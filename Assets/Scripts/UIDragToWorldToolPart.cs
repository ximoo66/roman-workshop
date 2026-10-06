// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIDragToWorldToolPart : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ToolPartType _partType;
    [SerializeField] private int _amountPerDrop = 1;

    [Header("World Raycast")]
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private LayerMask _assemblyMask = ~0;
    [SerializeField] private float _maxDistance = 25f;

    [Header("Drag Visual")]
    [SerializeField] private Canvas _dragCanvas;

    [Header("Audio")]
    [SerializeField] private AudioClip _audioClip;

    private GameObject _dragIconObject;
    private RectTransform _dragIconRect;
    private Image _sourceImage;

    private void Awake()
    {
        _sourceImage = GetComponent<Image>();
        if (_worldCamera == null) _worldCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.GetToolPartCount(_partType) < _amountPerDrop) return;

        CreateDragIcon();
        UpdateDragIconPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIconObject == null) return;
        UpdateDragIconPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragIconObject == null) return;

        TryDropToWorld(eventData.position);

        Destroy(_dragIconObject);
        _dragIconObject = null;
    }

    private void CreateDragIcon()
    {
        if (_dragCanvas == null)
        {
            Debug.LogError("UIDragToWorldToolPart: DragCanvas not assigned.");
            return;
        }

        _dragIconObject = new GameObject($"Drag_ToolPart_{_partType}");
        _dragIconObject.transform.SetParent(_dragCanvas.transform, false);

        _dragIconRect = _dragIconObject.AddComponent<RectTransform>();
        _dragIconRect.sizeDelta = new Vector2(90f, 90f);

        Image img = _dragIconObject.AddComponent<Image>();
        img.sprite = _sourceImage != null ? _sourceImage.sprite : null;
        img.raycastTarget = false;

        CanvasGroup cg = _dragIconObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = 0.9f;
    }

    private void UpdateDragIconPosition(Vector2 screenPosition)
    {
        if (_dragIconRect == null) return;
        _dragIconRect.position = screenPosition;
    }

    private void TryDropToWorld(Vector2 screenPosition)
    {
        if (_worldCamera == null) return;

        Ray ray = _worldCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _assemblyMask, QueryTriggerInteraction.Collide))
            return;

        ToolAssemblyStation station = hit.collider.GetComponentInParent<ToolAssemblyStation>();
        if (station == null)
            return;

        if (!station.CanAcceptDropFromRaycastHit(hit))
            return;

        station.TryPlacePart(_partType, _amountPerDrop);

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
