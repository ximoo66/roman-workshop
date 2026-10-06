// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;

public sealed class AssemblyCompleteHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _congratsUI;

    [Header("Saw Animation (Trigger-based)")]
    [SerializeField] private Animator _sawAnimator;
    [SerializeField] private string _startLoopTrigger = "StartLoop";

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _completeClip;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;

    [Header("Optional")]
    [SerializeField] private bool _onlyOnce = true;

    private bool _hasRun;

    public void OnAssemblyComplete()
    {
        if (_onlyOnce && _hasRun)
            return;

        _hasRun = true;

        if (_congratsUI != null)
            _congratsUI.SetActive(true);

        PlaySoundOnce();
        StartSawLoop();
    }

    private void StartSawLoop()
    {
        if (_sawAnimator == null)
            return;

        // Reset then fire to guarantee the transition works
        _sawAnimator.ResetTrigger(_startLoopTrigger);
        _sawAnimator.SetTrigger(_startLoopTrigger);
    }

    private void PlaySoundOnce()
    {
        if (_completeClip == null)
            return;

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.loop = false;
                _audioSource.spatialBlend = 0f; // 2D sound
            }
        }

        _audioSource.Stop();
        _audioSource.PlayOneShot(_completeClip, _volume);
    }
}
