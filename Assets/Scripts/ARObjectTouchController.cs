using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
#if UNITY_AR_FOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
#endif

public class ARObjectTouchController : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI debugText;

    [Header("Settings")]
    public float rotationSpeed = 90f;
    public LayerMask groundLayerMask = ~0;
    public float raycastDistance = 10f;
    [Tooltip("Minimum screen movement (pixels) required to start dragging")]
    public float dragThreshold = 20f;

    private Camera mainCam;
    private bool isDragging = false;
    private bool isPressing = false;
    private int activeFingerId = -1;
    private Vector2 initialTouchPos;
    private Vector3 offsetLocal = Vector3.zero;

#if UNITY_AR_FOUNDATION
    private ARRaycastManager arRaycastManager;
    static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
#endif

    private Transform target;

    void Awake()
    {
        mainCam = Camera.main;
        target = transform;
#if UNITY_AR_FOUNDATION
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
#endif
    }

    void Update()
    {
        HandleInput();

        // Rotate only when pressing and NOT dragging
        if (isPressing && !isDragging)
            target.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        UpdateDebugText();
    }

    void HandleInput()
    {
        // ignore UI touches
        if (Input.touchCount > 0)
        {
            Touch t0 = Input.GetTouch(0);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t0.fingerId)) return;
        }
        else
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) { }
        }

        if (Input.touchSupported && Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    if (IsTouchOverTarget(touch.position))
                    {
                        isPressing = true;
                        activeFingerId = touch.fingerId;
                        initialTouchPos = touch.position;
                        offsetLocal = Vector3.zero; // will set when drag actually starts
                    }
                }
                else if (touch.fingerId == activeFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        // If movement exceeds threshold, start dragging
                        if (!isDragging && Vector2.Distance(initialTouchPos, touch.position) >= dragThreshold)
                        {
                            isDragging = true;
                            // compute offset now using current touch pos
                            SetOffsetFromTouch(touch.position);
                        }

                        if (isDragging)
                            MoveTargetToScreenPoint(touch.position);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        // stop pressing and dragging
                        isPressing = false;
                        isDragging = false;
                        activeFingerId = -1;
                    }
                }
            }
        }
        else
        {
            // Mouse (editor)
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mpos = Input.mousePosition;
                if (IsTouchOverTarget(mpos))
                {
                    isPressing = true;
                    initialTouchPos = mpos;
                    offsetLocal = Vector3.zero;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 mpos = Input.mousePosition;
                if (!isDragging && Vector2.Distance(initialTouchPos, mpos) >= dragThreshold)
                {
                    isDragging = true;
                    SetOffsetFromTouch(mpos);
                }

                if (isDragging) MoveTargetToScreenPoint(mpos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isPressing = false;
                isDragging = false;
            }
        }
    }

    bool IsTouchOverTarget(Vector2 screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.transform == target || hit.transform.IsChildOf(target)) return true;
        }
        return false;
    }

    void SetOffsetFromTouch(Vector2 screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayerMask))
        {
            offsetLocal = target.position - hit.point;
        }
#if UNITY_AR_FOUNDATION
        else if (arRaycastManager != null && arRaycastManager.Raycast(screenPos, s_Hits, TrackableType.Planes))
        {
            Pose p = s_Hits[0].pose;
            offsetLocal = target.position - p.position;
        }
#endif
        else
        {
            // fallback: compute offset along camera forward at target's Y
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, target.position.y, 0));
            float enter;
            if (groundPlane.Raycast(ray, out enter))
            {
                Vector3 point = ray.GetPoint(enter);
                offsetLocal = target.position - point;
            }
            else offsetLocal = Vector3.zero;
        }
    }

    void MoveTargetToScreenPoint(Vector2 screenPos)
    {
#if UNITY_AR_FOUNDATION
        if (arRaycastManager != null && arRaycastManager.Raycast(screenPos, s_Hits, TrackableType.Planes))
        {
            Pose p = s_Hits[0].pose;
            Vector3 finalPos = p.position + offsetLocal;
            finalPos.y = p.position.y + offsetLocal.y;
            target.position = finalPos;
            return;
        }
#endif
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayerMask))
        {
            Vector3 finalPos = hit.point + offsetLocal;
            finalPos.y = hit.point.y + offsetLocal.y;
            target.position = finalPos;
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, target.position.y, 0));
            float enter;
            if (groundPlane.Raycast(ray, out enter))
            {
                Vector3 point = ray.GetPoint(enter);
                target.position = point + offsetLocal;
            }
        }
    }

    void UpdateDebugText()
    {
        if (debugText == null || target == null) return;
        Vector3 p = target.position;
        Vector3 e = target.eulerAngles;
        debugText.text = $"Pos: ({p.x:0.00}, {p.y:0.00}, {p.z:0.00})\nRot: ({e.x:0.0}, {e.y:0.0}, {e.z:0.0})";
    }
}
