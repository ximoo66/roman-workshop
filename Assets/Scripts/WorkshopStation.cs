// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using System.Collections;
using UnityEngine;

public sealed class WorkshopStation : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private Animator _axeAnimator;
    [SerializeField] private string _craftTriggerName = "Craft";
    [SerializeField] private ParticleSystem _craftParticles;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField, Min(0.1f)] private float _craftCooldownSeconds = 0.75f;

    [Header("Particles Toggle")]
    [SerializeField, Min(0.1f)] private float _particlesVisibleSeconds = 2f;

    [Header("Requirements")]
    [SerializeField, Min(1)] private int _woodRequired = 9;   // Logs
    [SerializeField, Min(1)] private int _rockRequired = 6;
    [SerializeField, Min(1)] private int _leafRequired = 3;
    [SerializeField, Min(1)] private int _ropeRequired = 1;

    [Header("Rewards")]
    [SerializeField] private ToolPartType[] _woodParts = new ToolPartType[5];
    [SerializeField] private ToolPartType[] _rockParts = new ToolPartType[3];
    [SerializeField] private ToolPartType[] _leafParts = new ToolPartType[1];

    [Tooltip("If rope should craft a part, set size to 1+ and assign the part(s). If rope is just a requirement with no parts, leave empty.")]
    [SerializeField] private ToolPartType[] _ropeParts = new ToolPartType[0];

    [Header("First Craft")]
    [Tooltip("Assign a GameObject to activate the very first time any craft completes.")]
    [SerializeField] private GameObject _firstCraftActivateObject;

    private int _woodDeposited;
    private int _rockDeposited;
    private int _leafDeposited;
    private int _ropeDeposited;

    private bool _craftedWood;
    private bool _craftedRock;
    private bool _craftedLeaf;
    private bool _craftedRope;

    // New flag: tracks whether the very first craft was already completed
    private bool _firstCraftCompleted;

    private bool _isCrafting;

    private Coroutine _particlesRoutine;
    private AudioSource _audioSource;

    private void Awake()
    {
        // Ensure particles are OFF at start
        if (_craftParticles != null)
            _craftParticles.gameObject.SetActive(false);

        // Cache / ensure AudioSource
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        // Ensure the first-craft object is off at start (if assigned)
        if (_firstCraftActivateObject != null)
            _firstCraftActivateObject.SetActive(false);

        UpdateProgressUI();
    }

    // ---------------- PUBLIC API ----------------
    public bool TryDepositOne(ItemType resourceType) => TryDeposit(resourceType, 1);

    public bool TryDeposit(ItemType resourceType, int amount)
    {
        if (amount <= 0)
        {
            DebugOverlay.Instance?.Set("Workshop: amount <= 0");
            return false;
        }

        if (_isCrafting)
        {
            DebugOverlay.Instance?.Set("Workshop: busy crafting (cooldown)");
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            DebugOverlay.Instance?.Set("Workshop: InventoryManager.Instance is NULL");
            return false;
        }

        // Accept resources used in crafting
        if (resourceType != ItemType.Log &&
            resourceType != ItemType.Rock &&
            resourceType != ItemType.Leaf &&
            resourceType != ItemType.Rope)
        {
            DebugOverlay.Instance?.Set($"Workshop: resource not accepted -> {resourceType}");
            return false;
        }

        if (IsAlreadyCrafted(resourceType))
        {
            DebugOverlay.Instance?.Set($"Workshop: already crafted for {resourceType}");
            return false;
        }

        // Consume from inventory
        if (!InventoryManager.Instance.TryConsumeResource(resourceType, amount))
        {
            int current = InventoryManager.Instance.GetResourceCount(resourceType);
            DebugOverlay.Instance?.Set($"Workshop: not enough {resourceType}. Have {current}, need {amount}.");
            return false;
        }

        // Update deposits + UI
        AddDeposit(resourceType, amount);
        UpdateProgressUI();

        DebugOverlay.Instance?.Set(
            $"Workshop: deposited {resourceType} (+{amount}) | " +
            $"Wood {_woodDeposited}/{_woodRequired}, " +
            $"Rock {_rockDeposited}/{_rockRequired}, " +
            $"Leaf {_leafDeposited}/{_leafRequired}, " +
            $"Rope {_ropeDeposited}/{_ropeRequired}"
        );

        TryCraft(resourceType);
        return true;
    }

    // ---------------- INTERNAL ----------------
    private bool IsAlreadyCrafted(ItemType t)
    {
        return t switch
        {
            ItemType.Log => _craftedWood,
            ItemType.Rock => _craftedRock,
            ItemType.Leaf => _craftedLeaf,
            ItemType.Rope => _craftedRope,
            _ => true
        };
    }

    private void AddDeposit(ItemType t, int amount)
    {
        switch (t)
        {
            case ItemType.Log: _woodDeposited += amount; break;
            case ItemType.Rock: _rockDeposited += amount; break;
            case ItemType.Leaf: _leafDeposited += amount; break;
            case ItemType.Rope: _ropeDeposited += amount; break;
        }
    }

    private void TryCraft(ItemType t)
    {
        switch (t)
        {
            case ItemType.Log:
                if (_craftedWood || _woodDeposited < _woodRequired) return;
                _craftedWood = true;
                StartCoroutine(CraftRoutine(_woodParts));
                break;

            case ItemType.Rock:
                if (_craftedRock || _rockDeposited < _rockRequired) return;
                _craftedRock = true;
                StartCoroutine(CraftRoutine(_rockParts));
                break;

            case ItemType.Leaf:
                if (_craftedLeaf || _leafDeposited < _leafRequired) return;
                _craftedLeaf = true;
                StartCoroutine(CraftRoutine(_leafParts));
                break;

            case ItemType.Rope:
                if (_craftedRope || _ropeDeposited < _ropeRequired) return;
                _craftedRope = true;
                StartCoroutine(CraftRoutine(_ropeParts));
                break;
        }
    }

    private IEnumerator CraftRoutine(ToolPartType[] parts)
    {
        _isCrafting = true;

        // Axe animation
        if (_axeAnimator != null && !string.IsNullOrWhiteSpace(_craftTriggerName))
            _axeAnimator.SetTrigger(_craftTriggerName);

        // Particles (enable -> play -> disable after X seconds)
        TriggerParticlesForSeconds();

        // Audio
        if (_audioClip != null && _audioSource != null)
            _audioSource.PlayOneShot(_audioClip, 1f);

        // Cooldown (also acts like craft duration)
        yield return new WaitForSeconds(_craftCooldownSeconds);

        // Add parts to inventory
        if (InventoryManager.Instance != null && parts != null && parts.Length > 0)
        {
            for (int i = 0; i < parts.Length; i++)
                InventoryManager.Instance.AddToolPart(parts[i], 1);
        }

        UpdateProgressUI();
        DebugOverlay.Instance?.Set("Workshop: crafting done -> parts added.");

        // New: if this is the very first craft ever, activate the assigned object (once)
        if (!_firstCraftCompleted)
        {
            _firstCraftCompleted = true;
            if (_firstCraftActivateObject != null)
                _firstCraftActivateObject.SetActive(true);
        }

        _isCrafting = false;
    }

    private void TriggerParticlesForSeconds()
    {
        if (_craftParticles == null)
            return;

        if (_particlesRoutine != null)
            StopCoroutine(_particlesRoutine);

        _particlesRoutine = StartCoroutine(ParticlesRoutine());
    }

    private IEnumerator ParticlesRoutine()
    {
        GameObject go = _craftParticles.gameObject;

        // Hard reset (handles cases where it was left active/playing)
        go.SetActive(false);
        go.SetActive(true);

        _craftParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _craftParticles.Play(true);

        yield return new WaitForSeconds(_particlesVisibleSeconds);

        go.SetActive(false);
        _particlesRoutine = null;
    }

    private void UpdateProgressUI()
    {
        if (WorkshopProgressUI.Instance == null)
            return;

        WorkshopProgressUI.Instance.SetProgress(
            _woodDeposited, _woodRequired,
            _rockDeposited, _rockRequired,
            _leafDeposited, _leafRequired,
            _ropeDeposited, _ropeRequired
        );
    }
}
