// Author: Omid Ameri
// Modified by: Julong Yan - Dynamic Progress now
// Course: P5 – Roman Workshop (AR Project)

using TMPro;
using UnityEngine;

public sealed class WorkshopProgressUI : MonoBehaviour
{
    public static WorkshopProgressUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        Instance = this;
        if (_text != null)
            _text.gameObject.SetActive(false);
    }

    public void SetProgress(
        int wood, int woodReq,
        int rock, int rockReq,
        int leaf, int leafReq,
        int rope, int ropeReq)
    {
        if (_text == null) return;

        var lines = new System.Collections.Generic.List<string>(5);

        AddLineIfStarted(lines, "Wood", wood, woodReq);
        AddLineIfStarted(lines, "Rock", rock, rockReq);
        AddLineIfStarted(lines, "Leaf", leaf, leafReq);
        AddLineIfStarted(lines, "Rope", rope, ropeReq);

        if (lines.Count == 0)
        {
            _text.gameObject.SetActive(false);
            return;
        }

        // Prepend header
        lines.Insert(0, "Now crafting:");

        _text.text = string.Join("\n", lines);
        _text.gameObject.SetActive(true);
    }

    public void SetVisible(bool visible)
    {
        if (_text != null)
            _text.gameObject.SetActive(visible);
    }

    private void AddLineIfStarted(System.Collections.Generic.List<string> lines, string name, int count, int req)
    {
        if (req <= 0) return;
        if (count <= 0) return;

        if (count >= req)
            lines.Add($"{name} finished");
        else
            lines.Add($"{name}: {count}/{req}");
    }
}
