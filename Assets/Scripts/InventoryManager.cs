// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Resource Slots (Fixed Order: Rope, Log, Rock, Leaf)")]
    [SerializeField] private InventorySlot[] _resourceSlots = new InventorySlot[4];

    [Header("Resource Sprites")]
    [SerializeField] private Sprite _rockIcon;
    [SerializeField] private Sprite _logIcon;
    [SerializeField] private Sprite _ropeIcon;
    [SerializeField] private Sprite _leafIcon;

    [Header("Tool Part Slots (UI)")]
    [SerializeField] private ToolPartSlot[] _toolPartSlots;

    [Header("Tool Part Sprites (same order as ToolPartType enum)")]
    [SerializeField] private Sprite[] _toolPartSprites;

    private readonly Dictionary<ItemType, int> _resourceCounts = new();
    private readonly Dictionary<ToolPartType, int> _partCounts = new();

    // Fixed mapping: item -> slot index
    // Slot 0 Rope, Slot 1 Log, Slot 2 Rock, Slot 3 Leaf
    private readonly Dictionary<ItemType, int> _fixedSlotIndex = new()
    {
        { ItemType.Rope, 0 },
        { ItemType.Log,  1 },
        { ItemType.Rock, 2 },
        { ItemType.Leaf, 3 }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            _resourceCounts[t] = 0;

        foreach (ToolPartType t in Enum.GetValues(typeof(ToolPartType)))
            _partCounts[t] = 0;

        SetupToolPartSlots();
        RefreshResourceUI();
        RefreshToolPartsUI();
    }

    // ---------------- RESOURCES ----------------
    public void AddItem(ItemType item) => AddResource(item, 1);

    public void AddResource(ItemType item, int amount)
    {
        if (amount <= 0) return;
        if (!_fixedSlotIndex.ContainsKey(item))
        {
            // Ignore types that are not part of the 4 resource UI slots (like Machine, etc.)
            _resourceCounts[item] += amount;
            return;
        }

        _resourceCounts[item] += amount;
        RefreshResourceUI();
    }

    public bool TryConsumeResource(ItemType item, int amount)
    {
        if (amount <= 0) return true;
        if (!_resourceCounts.ContainsKey(item)) return false;
        if (_resourceCounts[item] < amount) return false;

        _resourceCounts[item] -= amount;
        RefreshResourceUI();
        return true;
    }

    public int GetResourceCount(ItemType item)
    {
        return _resourceCounts.TryGetValue(item, out int c) ? c : 0;
    }

    private void RefreshResourceUI()
    {
        // Enforce fixed slot content always
        ApplyResourceSlot(0, ItemType.Rope, _ropeIcon);
        ApplyResourceSlot(1, ItemType.Log, _logIcon);
        ApplyResourceSlot(2, ItemType.Rock, _rockIcon);
        ApplyResourceSlot(3, ItemType.Leaf, _leafIcon);
    }

    private void ApplyResourceSlot(int slotIndex, ItemType item, Sprite icon)
    {
        if (_resourceSlots == null || slotIndex < 0 || slotIndex >= _resourceSlots.Length)
            return;

        InventorySlot slot = _resourceSlots[slotIndex];
        if (slot == null) return;

        int count = GetResourceCount(item);

        // If you want icons ALWAYS visible, remove the "count <= 0" hiding part.
        if (count <= 0)
        {
            slot.iconImage.enabled = false;
            slot.countText.text = "";
            return;
        }

        slot.iconImage.enabled = true;
        slot.iconImage.sprite = icon;
        slot.countText.text = count.ToString();
    }

    // ---------------- TOOL PARTS ----------------
    public void AddToolPart(ToolPartType part, int amount = 1)
    {
        if (amount <= 0) return;

        _partCounts[part] += amount;
        RefreshToolPartsUI();
    }

    public int GetToolPartCount(ToolPartType part)
    {
        return _partCounts.TryGetValue(part, out int count) ? count : 0;
    }

    public bool TryConsumeToolPart(ToolPartType part, int amount)
    {
        if (amount <= 0) return true;

        int current = GetToolPartCount(part);
        if (current < amount)
            return false;

        _partCounts[part] = current - amount;
        RefreshToolPartsUI();
        return true;
    }

    private void SetupToolPartSlots()
    {
        if (_toolPartSlots == null) return;

        for (int i = 0; i < _toolPartSlots.Length; i++)
        {
            ToolPartSlot slot = _toolPartSlots[i];
            if (slot == null) continue;

            Sprite icon = GetToolPartSprite(slot.PartType);
            slot.Setup(icon);
        }
    }

    private Sprite GetToolPartSprite(ToolPartType part)
    {
        int idx = (int)part;
        if (_toolPartSprites == null || idx < 0 || idx >= _toolPartSprites.Length)
            return null;

        return _toolPartSprites[idx];
    }

    private void RefreshToolPartsUI()
    {
        if (_toolPartSlots == null) return;

        for (int i = 0; i < _toolPartSlots.Length; i++)
        {
            ToolPartSlot s = _toolPartSlots[i];
            if (s == null) continue;

            s.SetCount(GetToolPartCount(s.PartType));
        }
    }
    public void ForceRefreshUI()
    {
        RefreshResourceUI();
        RefreshToolPartsUI();
    }

}
