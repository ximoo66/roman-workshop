// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;

public sealed class CollectOnTap : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    [SerializeField, Min(1)] private int _value = 1;
    [SerializeField] private AudioClip _pickupSfx; // assign in inspector
    [SerializeField] private float _sfxVolume = 1f;

    // New: assign in inspector the GameObject to activate on the very first collect
    [SerializeField] private GameObject _firstCollectActivate;

    // Static flag shared across all instances to ensure this runs only once
    private static bool s_hasDoneFirstCollect;

    private bool _collected;

    public void TryCollect()
    {
        if (_collected) return;
        _collected = true;

        // Activate the first-collect object only once, globally
        if (!s_hasDoneFirstCollect && _firstCollectActivate != null)
        {
            _firstCollectActivate.SetActive(true);
            s_hasDoneFirstCollect = true;
        }

        // Play pickup sound (create a temporary AudioSource if needed)
        if (_pickupSfx != null)
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            audioSource.PlayOneShot(_pickupSfx, _sfxVolume);
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[CollectOnTap] InventoryManager.Instance is NULL. Vanishing anyway.");
            // allow sound to finish (optional): destroy after clip length if playing
            if (_pickupSfx != null)
            {
                Destroy(gameObject, _pickupSfx.length);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }

        InventoryManager.Instance.AddResource(_itemType, _value);

        // If a clip was played, delay destroy so it can be heard; otherwise destroy immediately
        if (_pickupSfx != null)
            Destroy(gameObject, _pickupSfx.length);
        else
            Destroy(gameObject);
    }
}
