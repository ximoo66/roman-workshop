// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class ToolAssemblyStation : MonoBehaviour
{
    [Serializable]
    private sealed class PartVisual
    {
        [SerializeField] private ToolPartType _partType;
        [SerializeField] private Renderer _targetRenderer;

        [Header("Materials")]
        [SerializeField] private Material _transparentMaterial;
        [SerializeField] private Material _completedMaterial;

        public ToolPartType PartType => _partType;

        public bool IsValid => _targetRenderer != null && _transparentMaterial != null && _completedMaterial != null;

        public void ApplyTransparent()
        {
            if (!IsValid) return;
            _targetRenderer.material = _transparentMaterial;
        }

        public void ApplyCompleted()
        {
            if (!IsValid) return;
            _targetRenderer.material = _completedMaterial;
        }
    }

    [Header("Drop Target")]
    [SerializeField] private Collider _dropCollider;

    [Header("Part Visuals (10 entries recommended)")]
    [SerializeField] private PartVisual[] _parts = new PartVisual[9];

    [Header("Feedback")]
    [SerializeField] private ParticleSystem _placeParticles;
    [SerializeField, Min(0.1f)] private float _particlesVisibleSeconds = 2f;

    [Tooltip("AudioSource to play on each successful placement. Should NOT be looping.")]
    [SerializeField] private AudioSource _placeAudio;

    [SerializeField] private UnityEvent _onAssemblyComplete;

    private readonly Dictionary<ToolPartType, PartVisual> _lookup = new();
    private readonly HashSet<ToolPartType> _placed = new();

    private Coroutine _particlesRoutine;

    private void Awake()
    {
        if (_dropCollider == null)
            _dropCollider = GetComponentInChildren<Collider>();

        // Start with particles OFF
        if (_placeParticles != null)
            _placeParticles.gameObject.SetActive(false);

        // Ensure audio is configured correctly
        if (_placeAudio != null)
        {
            _placeAudio.playOnAwake = false;
            _placeAudio.loop = false;
        }

        _lookup.Clear();
        _placed.Clear();

        for (int i = 0; i < _parts.Length; i++)
        {
            PartVisual p = _parts[i];
            if (p == null || !p.IsValid) continue;

            if (!_lookup.ContainsKey(p.PartType))
                _lookup.Add(p.PartType, p);

            p.ApplyTransparent();
        }
    }

    // Compatibility: your UI drag was calling this name
    public bool CanAcceptDropFromRaycastHit(RaycastHit hit)
    {
        if (_dropCollider == null) return false;
        return hit.collider == _dropCollider || hit.collider.transform.IsChildOf(_dropCollider.transform);
    }

    public bool TryPlacePart(ToolPartType partType, int amount = 1)
    {
        if (amount <= 0) return false;
        if (InventoryManager.Instance == null) return false;

        if (_placed.Contains(partType))
            return false;

        if (!_lookup.TryGetValue(partType, out PartVisual visual) || visual == null || !visual.IsValid)
            return false;

        if (!InventoryManager.Instance.TryConsumeToolPart(partType, amount))
            return false;

        _placed.Add(partType);
        visual.ApplyCompleted();

        // Feedback
        TriggerPlaceParticlesForSeconds();
        PlayPlaceAudioOnce();

        if (IsComplete())
            _onAssemblyComplete?.Invoke();

        return true;
    }

    private void TriggerPlaceParticlesForSeconds()
    {
        if (_placeParticles == null)
            return;

        if (_particlesRoutine != null)
            StopCoroutine(_particlesRoutine);

        _particlesRoutine = StartCoroutine(PlaceParticlesRoutine());
    }

    private IEnumerator PlaceParticlesRoutine()
    {
        GameObject go = _placeParticles.gameObject;

        // Reset to ensure a clean one-shot
        go.SetActive(false);
        go.SetActive(true);

        _placeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _placeParticles.Play(true);

        yield return new WaitForSeconds(_particlesVisibleSeconds);

        go.SetActive(false);
        _particlesRoutine = null;
    }

    private void PlayPlaceAudioOnce()
    {
        if (_placeAudio == null)
            return;

        // If a previous one-shot is still playing, restart for snappy feedback
        _placeAudio.Stop();
        _placeAudio.Play();
    }

    private bool IsComplete()
    {
        int needed = 0;
        for (int i = 0; i < _parts.Length; i++)
            if (_parts[i] != null && _parts[i].IsValid) needed++;

        return _placed.Count >= needed;
    }
}
