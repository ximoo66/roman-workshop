// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;

public sealed class SimpleToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiPanel;
    [SerializeField] private GameObject _lookAround;
    [SerializeField] private bool _hideLookAroundOnlyOnce = true;
    // New fields for the one-shot sound
    [SerializeField] private AudioClip _toggleClip;
    [SerializeField] private float _sfxVolume = 1f;
    private bool _didHideLookAround;

    public void TogglePanel()
    {
        if (_uiPanel == null)
        {
            Debug.LogError("[SimpleToggleButton] UI Panel reference is missing.");
            return;
        }

        bool willOpen = !_uiPanel.activeSelf;
        _uiPanel.SetActive(willOpen);

        if (willOpen)
        {
            // Optional: refresh UI when opening so counts/icons are always correct
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.ForceRefreshUI();

            // Play pickup sound (create a temporary AudioSource if needed)
            if (_toggleClip != null)
            {
                var audioSource = gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
                audioSource.PlayOneShot(_toggleClip, _sfxVolume);
            }
        }

        if (_hideLookAroundOnlyOnce && !_didHideLookAround && _lookAround != null)
        {
            _lookAround.SetActive(false);
            _didHideLookAround = true;
        }
    }
}
