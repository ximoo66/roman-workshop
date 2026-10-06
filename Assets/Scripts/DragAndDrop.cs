// Author: Julong Yan
// Stolen from: Omid Ameri
// Experiential !
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Optional UI")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Spawn settings (assign in Inspector per icon or prefab)")]
    [Tooltip("Camera used to convert screen point to world ray.")]
    [SerializeField] private Camera _arCamera;
    [Tooltip("Prefab to spawn in the world when dropping this icon.")]
    [SerializeField] private GameObject _worldCubePrefab;
    [Tooltip("Distance along ray to spawn the prefab if no hit.")]
    [SerializeField] private float _spawnDistance = 1f;

    [Header("Item data (set per icon instance)")]
    [SerializeField] private ItemType _itemType;
    [SerializeField] private int _itemCount = 1; // how many this icon represents

    private RectTransform _rectTransform;
    private Vector2 _startAnchoredPos;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_rectTransform != null)
        {
            _rectTransform.localRotation = Quaternion.identity;
            _rectTransform.localScale = Vector3.one;
        }
    }

    // Optional convenience initializer to set camera/prefab/count from code
    public void Initialise(Camera arCamera, GameObject worldPrefab, float spawnDistance, ItemType itemType, int itemCount = 1)
    {
        _arCamera = arCamera;
        _worldCubePrefab = worldPrefab;
        _spawnDistance = spawnDistance;
        _itemType = itemType;
        _itemCount = itemCount;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_rectTransform == null) return;
        _startAnchoredPos = _rectTransform.anchoredPosition;
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.8f;
        }
        Debug.Log("DragAndDrop: Begin drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_rectTransform == null) return;
        _rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }

        Debug.Log("DragAndDrop: End drag at " + eventData.position);

        bool spawned = TrySpawnAtScreenPosition(eventData.position);

        if (!spawned)
        {
            // Snap back if spawn failed
            if (_rectTransform != null) _rectTransform.anchoredPosition = _startAnchoredPos;
        }
        else
        {
            // Decrement local count and destroy icon when consumed
            _itemCount--;
            if (_itemCount <= 0)
                Destroy(gameObject);
            else
                UpdateIconCountUI();
        }
    }

    // Tries to spawn the world prefab. Returns true on success.
    private bool TrySpawnAtScreenPosition(Vector2 screenPos)
    {
        if (_arCamera == null)
        {
            Debug.LogError("DragAndDrop: AR Camera not assigned.");
            return false;
        }
        if (_worldCubePrefab == null)
        {
            Debug.LogError("DragAndDrop: World prefab not assigned.");
            return false;
        }
        if (_itemCount <= 0)
        {
            Debug.LogWarning("DragAndDrop: no items left to spawn.");
            return false;
        }

        // Raycast into world; spawn at hit point if hit something, otherwise at fixed distance
        Ray ray = _arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Instantiate(_worldCubePrefab, hit.point, Quaternion.identity);
            Debug.Log("DragAndDrop: Spawned at hit point " + hit.point);
        }
        else
        {
            Vector3 worldPos = ray.GetPoint(_spawnDistance);
            Instantiate(_worldCubePrefab, worldPos, Quaternion.identity);
            Debug.Log("DragAndDrop: Spawned at distance " + worldPos);
        }

        return true;
    }

    // If your icon UI has a count text, update it here. Implement as needed.
    private void UpdateIconCountUI()
    {
        // Example: if you have a child Text or TMP component, update it.
        // This method is left empty so you can wire your UI specifics.
    }
}
