// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public sealed class QRAnchor : MonoBehaviour
{
    [Header("Testing Helpers")]
    [SerializeField] private bool _indoorTestingSpawnInFrontOfCamera = true;
    [SerializeField, Min(0.25f)] private float _spawnDistanceFromCamera = 1.2f;
    [SerializeField] private float _spawnHeightOffset = -0.1f;
    [SerializeField] private bool _faceCameraOnSpawn = true;

    [SerializeField] private float _forcedWorldScale = 1f; // e.g. 1.0, 0.5 if too big

    [Header("AR")]
    [SerializeField] private ARTrackedImageManager _imageManager;

    [Header("Anchor & Content (snap once)")]
    [SerializeField] private Transform _worldAnchorRoot; // child of XR Origin (AR Mobile)
    [SerializeField] private GameObject _qrContent;      // parent of materials (will be parented to anchor)

    [Header("Target")]
    [SerializeField] private string _targetImageName = "QR_Info_01";

    [Header("Optional: Lock tracking after first scan (recommended)")]
    [SerializeField] private bool _disableImageTrackingAfterLock = true;

    [Header("Optional UI Feedback")]
    [SerializeField] private GameObject _scanOkFeedback;
    [SerializeField] private float _feedbackDurationSeconds = 1.5f;

    [Header("Hint Text")]
    [SerializeField] private GameObject _optionalText;
    [SerializeField] private GameObject _nextOptionalText;

    [Header("Inventory Icon")]
    [SerializeField] private GameObject _inventoryIcon;

    [Header("Mask Screen")]
    [SerializeField] private GameObject _maskScreen;

    [Header("LookAround Screen")]
    [SerializeField] private GameObject _lookaroundScreen;

    [Header("Progress Panel")]
    [SerializeField] private GameObject _progressPanel;

    [Header("Fixed Assembly Tool (SNAP ONCE, NO FOLLOW)")]
    [SerializeField] private Transform _fixedAssemblyTool;
    [SerializeField] private bool _spawnToolOnScan = true;
    [SerializeField] private Vector3 _toolOffsetFromQR = new Vector3(0f, 0f, 2f);
    [SerializeField] private Vector3 _toolEulerRotation = new Vector3(0f, 180f, 0f);

    [Header("Fixed Workshop Station (SNAP ONCE, NO FOLLOW)")]
    [SerializeField] private Transform _fixedWorkshopStation;
    [SerializeField] private bool _spawnWorkshopOnScan = true;
    [SerializeField] private Vector3 _workshopOffsetFromQR = new Vector3(1.5f, 0f, 1.0f);
    [SerializeField] private Vector3 _workshopEulerRotation = new Vector3(0f, 180f, 0f);

    private bool _hasAnchor;
    private bool _feedbackVisible;
    private float _feedbackTimer;

    private bool _toolSpawnedOnce;
    private bool _workshopSpawnedOnce;

    private void OnEnable()
    {
#pragma warning disable CS0618
        if (_imageManager != null)
            _imageManager.trackedImagesChanged += OnTrackedImagesChanged;
#pragma warning restore CS0618
    }

    private void OnDisable()
    {
#pragma warning disable CS0618
        if (_imageManager != null)
            _imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
#pragma warning restore CS0618
    }

#pragma warning disable CS0618
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
#pragma warning restore CS0618
    {
        // If already locked, ignore all further updates.
        if (_hasAnchor)
            return;

        foreach (var added in args.added)
            TryLockToImage(added);

        foreach (var updated in args.updated)
            TryLockToImage(updated);
    }

    private void TryLockToImage(ARTrackedImage tracked)
    {
        if (_hasAnchor)
            return;

        if (tracked.referenceImage.name != _targetImageName)
            return;

        if (tracked.trackingState != TrackingState.Tracking)
            return;

        if (_worldAnchorRoot == null)
            return;

        // 1) SNAP ONCE: lock anchor to the tracked image pose
        _worldAnchorRoot.position = tracked.transform.position;
        _worldAnchorRoot.rotation = tracked.transform.rotation;

        // 2) Parent QR-follow content under anchor ONCE (no further follow)
        if (_qrContent != null)
        {
            _qrContent.transform.SetParent(_worldAnchorRoot, true);
            _qrContent.SetActive(true);
        }

        // 3) UI toggles
        if (_optionalText != null) _optionalText.SetActive(false);
        if (_nextOptionalText != null) _nextOptionalText.SetActive(true);
        if (_inventoryIcon != null) _inventoryIcon.SetActive(true);
        if (_maskScreen != null) _maskScreen.SetActive(false);
        if (_progressPanel != null) _progressPanel.SetActive(true);
        if (_lookaroundScreen != null) _lookaroundScreen.SetActive(true);

        // 4) Spawn fixed objects ONCE relative to QR pose
        TrySpawnFixedAssemblyTool(tracked);
        TrySpawnFixedWorkshopStation(tracked);

        // 5) Give user a free rope (safe-guard against null instance)
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddResource(ItemType.Rope, 1);

        _hasAnchor = true;
        ShowFeedbackOnce();

        // 6) Kill jitter: stop image tracking updates after lock
        if (_disableImageTrackingAfterLock && _imageManager != null)
            _imageManager.enabled = false;
    }

    private void TrySpawnFixedAssemblyTool(ARTrackedImage tracked)
    {
        if (!_spawnToolOnScan) return;
        if (_toolSpawnedOnce) return;
        if (_fixedAssemblyTool == null) return;

        _fixedAssemblyTool.gameObject.SetActive(true);

        Pose spawnPose = GetSpawnPose(tracked, _toolOffsetFromQR);

        _fixedAssemblyTool.position = spawnPose.position;
        _fixedAssemblyTool.rotation = spawnPose.rotation * Quaternion.Euler(_toolEulerRotation);
        _fixedAssemblyTool.localScale = Vector3.one * _forcedWorldScale;

        if (_worldAnchorRoot != null)
            _fixedAssemblyTool.SetParent(_worldAnchorRoot, true);

        _toolSpawnedOnce = true;
    }

    private void TrySpawnFixedWorkshopStation(ARTrackedImage tracked)
    {
        if (!_spawnWorkshopOnScan) return;
        if (_workshopSpawnedOnce) return;
        if (_fixedWorkshopStation == null) return;

        _fixedWorkshopStation.gameObject.SetActive(true);

        Pose spawnPose = GetSpawnPose(tracked, _workshopOffsetFromQR);

        _fixedWorkshopStation.position = spawnPose.position;
        _fixedWorkshopStation.rotation = spawnPose.rotation * Quaternion.Euler(_workshopEulerRotation);
        _fixedWorkshopStation.localScale = Vector3.one * _forcedWorldScale;

        if (_worldAnchorRoot != null)
            _fixedWorkshopStation.SetParent(_worldAnchorRoot, true);

        _workshopSpawnedOnce = true;
    }

    private Pose GetSpawnPose(ARTrackedImage tracked, Vector3 qrLocalOffset)
    {
        // Indoor testing: place in front of camera so you can reach it in a room
        if (_indoorTestingSpawnInFrontOfCamera && Camera.main != null)
        {
            Transform cam = Camera.main.transform;
            Vector3 pos = cam.position + cam.forward * _spawnDistanceFromCamera;
            pos.y += _spawnHeightOffset;

            Quaternion rot = _faceCameraOnSpawn
                ? Quaternion.LookRotation(new Vector3(cam.forward.x, 0f, cam.forward.z).normalized, Vector3.up)
                : tracked.transform.rotation;

            return new Pose(pos, rot);
        }

        // Default: QR-relative placement
        Vector3 worldOffset = tracked.transform.TransformDirection(qrLocalOffset);
        return new Pose(tracked.transform.position + worldOffset, tracked.transform.rotation);
    }


    private void Update()
    {
        if (!_feedbackVisible)
            return;

        _feedbackTimer -= Time.deltaTime;
        if (_feedbackTimer <= 0f)
        {
            _feedbackVisible = false;
            if (_scanOkFeedback != null)
                _scanOkFeedback.SetActive(false);
        }
    }

    private void ShowFeedbackOnce()
    {
        if (_scanOkFeedback == null)
            return;

        _scanOkFeedback.SetActive(true);
        _feedbackVisible = true;
        _feedbackTimer = _feedbackDurationSeconds;
    }
}
