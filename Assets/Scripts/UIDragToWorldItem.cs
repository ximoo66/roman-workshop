// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIDragToWorldItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item")]
    [SerializeField] private ItemType _itemType = ItemType.Log;
    [SerializeField, Min(1)] private int _amountPerDrop = 1;

    [Header("World Raycast")]
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private LayerMask _dropLayerMask = ~0; // Everything by default
    [SerializeField] private float _maxRayDistance = 15000f;

    [Header("Drag Visual")]
    [SerializeField] private Canvas _dragCanvas; // Screen Space - Overlay recommended

    [Header("Audio")]
    [SerializeField] private AudioClip _audioClip;

    private GameObject _dragIconObject;
    private RectTransform _dragIconRect;
    private Image _sourceImage;

    private void Awake()
    {
        _sourceImage = GetComponent<Image>();

        if (_worldCamera == null)
            _worldCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            DebugOverlay.Instance?.Set($"Can't drag {_itemType}: not enough in inventory.");
            return;
        }

        if (_dragCanvas == null)
        {
            DebugOverlay.Instance?.Set("Drag failed: DragCanvas not assigned.");
            return;
        }

        CreateDragIcon();
        UpdateDragIconPosition(eventData.position);
        DebugOverlay.Instance?.Set($"Dragging: {_itemType}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIconObject == null)
            return;

        UpdateDragIconPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragIconObject == null)
            return;

        TryDropToWorld(eventData.position);

        Destroy(_dragIconObject);
        _dragIconObject = null;
    }

    private bool CanDrag()
    {
        if (InventoryManager.Instance == null)
            return false;

        return InventoryManager.Instance.GetResourceCount(_itemType) >= _amountPerDrop;
    }

    private void CreateDragIcon()
    {
        _dragIconObject = new GameObject($"DragIcon_{_itemType}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        _dragIconObject.transform.SetParent(_dragCanvas.transform, false);

        _dragIconRect = _dragIconObject.GetComponent<RectTransform>();
        _dragIconRect.sizeDelta = new Vector2(90f, 90f);

        Image img = _dragIconObject.GetComponent<Image>();
        img.sprite = _sourceImage != null ? _sourceImage.sprite : null;
        img.raycastTarget = false;

        CanvasGroup cg = _dragIconObject.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = 0.9f;
    }

    private void UpdateDragIconPosition(Vector2 screenPosition)
    {
        if (_dragIconRect == null)
            return;

        _dragIconRect.position = screenPosition;
    }

    private void TryDropToWorld(Vector2 screenPosition)
    {
        if (_worldCamera == null)
        {
            _worldCamera = Camera.main;
            if (_worldCamera == null)
            {
                DebugOverlay.Instance?.Set("Drop failed: WorldCamera is NULL (assign AR Camera or tag it MainCamera).");
                return;
            }
        }

        Ray ray = _worldCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _dropLayerMask, QueryTriggerInteraction.Collide))
        {
            DebugOverlay.Instance?.Set("Drop failed: Raycast hit NOTHING (no collider / too far / wrong layer).");
            return;
        }

        WorkshopStation station = hit.collider.GetComponentInParent<WorkshopStation>();
        if (station == null)
        {
            DebugOverlay.Instance?.Set($"Drop hit {hit.collider.name}, but NO WorkshopStation found.");
            return;
        }

        bool ok = station.TryDeposit(_itemType, _amountPerDrop);
        DebugOverlay.Instance?.Set(ok
            ? $"Deposit OK: {_itemType} x{_amountPerDrop}"
            : $"Deposit FAILED: {_itemType} (see workshop reason)");

        // Play pickup sound (create a temporary AudioSource if needed)
        if (_audioClip != null)
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            audioSource.PlayOneShot(_audioClip, 1f);
        }

    }
}
