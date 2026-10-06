using UnityEngine;

public class ResourceHideController : MonoBehaviour
{
    [Header("Thresholds")]
    [Tooltip("Number of logs required to hide the log image")]
    public int logsToDisappear = 4;
    [Tooltip("Number of rocks required to hide the rock image")]
    public int rocksToDisappear = 4;
    [Tooltip("Number of leaves required to hide the leaf image")]
    public int leavesToDisappear = 4;

    [Header("UI References")]
    public GameObject logImage;
    public GameObject rockImage;
    public GameObject leafImage;
    public GameObject panel;

    // Optional: how often to poll (in seconds). Set to 0 to check every frame.
    public float pollInterval = 0.2f;

    private float _nextPollTime;

    // Persistent runtime flags: once true, that resource image will never be re-shown.
    private bool _logLockedHidden;
    private bool _rockLockedHidden;
    private bool _leafLockedHidden;

    private void OnEnable()
    {
        // Immediate check on enable
        CheckAndUpdateUI();
    }

    private void Update()
    {
        if (pollInterval <= 0f)
        {
            CheckAndUpdateUI();
            return;
        }

        if (Time.unscaledTime >= _nextPollTime)
        {
            _nextPollTime = Time.unscaledTime + Mathf.Max(0.01f, pollInterval);
            CheckAndUpdateUI();
        }
    }

    private void CheckAndUpdateUI()
    {
        // Safety: do nothing if InventoryManager not available
        if (InventoryManager.Instance == null) return;

        int logs = InventoryManager.Instance.GetResourceCount(ItemType.Log);
        int rocks = InventoryManager.Instance.GetResourceCount(ItemType.Rock);
        int leaves = InventoryManager.Instance.GetResourceCount(ItemType.Leaf);

        bool logShouldHide = logs >= Mathf.Max(0, logsToDisappear);
        bool rockShouldHide = rocks >= Mathf.Max(0, rocksToDisappear);
        bool leafShouldHide = leaves >= Mathf.Max(0, leavesToDisappear);

        // Once a resource is marked lockedHidden, it stays true.
        if (logShouldHide) _logLockedHidden = true;
        if (rockShouldHide) _rockLockedHidden = true;
        if (leafShouldHide) _leafLockedHidden = true;

        // Apply visibility: if lockedHidden is true, the image is hidden and will never be shown.
        if (logImage != null)
        {
            // Only set inactive when locked; never set active if locked.
            if (_logLockedHidden) logImage.SetActive(false);
            else logImage.SetActive(true);
        }

        if (rockImage != null)
        {
            if (_rockLockedHidden) rockImage.SetActive(false);
            else rockImage.SetActive(true);
        }

        if (leafImage != null)
        {
            if (_leafLockedHidden) leafImage.SetActive(false);
            else leafImage.SetActive(true);
        }

        // Panel hides when all three resources are locked hidden.
        bool allLockedHidden = _logLockedHidden && _rockLockedHidden && _leafLockedHidden;
        if (panel != null) panel.SetActive(!allLockedHidden);
    }

    // Public helper to force an immediate check (e.g., call from other scripts)
    public void ForceRefresh() => CheckAndUpdateUI();

    // Optional: allow manual reset of locked flags if you ever need to re-enable images.
    // Call ResetLocks() from code if you want to make images re-showable again.
    public void ResetLocks()
    {
        _logLockedHidden = false;
        _rockLockedHidden = false;
        _leafLockedHidden = false;
        // Immediately refresh UI to reflect unlocked state
        CheckAndUpdateUI();
    }
}
