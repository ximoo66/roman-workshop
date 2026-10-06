using TMPro;
using UnityEngine;

public sealed class DebugOverlay : MonoBehaviour
{
    public static DebugOverlay Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        Instance = this;
        Set("DebugOverlay ready");
    }

    public void Set(string msg)
    {
        if (_text != null)
            _text.text = msg;
    }
}
