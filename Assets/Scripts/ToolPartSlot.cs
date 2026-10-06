// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ToolPartSlot : MonoBehaviour
{
    [SerializeField] private ToolPartType _partType;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;

    public ToolPartType PartType => _partType;

    public void Setup(Sprite icon)
    {
        _iconImage.sprite = icon;
        gameObject.SetActive(false);
        if (_countText != null) _countText.text = "";
    }

    public void SetCount(int count)
    {
        bool show = count > 0;
        gameObject.SetActive(show);

        if (_countText != null)
            _countText.text = show ? count.ToString() : "";
    }
}
