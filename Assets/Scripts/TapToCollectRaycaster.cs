// Author: Omid Ameri
// Course: P5 – Roman Workshop (AR Project)

using System;
using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class TapToCollectRaycaster : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _maxDistance = 1000f;

    [Header("Debug UI")]
    [SerializeField] private TextMeshProUGUI _debugText;

    private void Awake()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();

        SetDebug("TapToCollect: READY");
    }

    private void Update()
    {
        if (!TryGetTapPosition(out Vector2 tapPos))
            return;

        SetDebug($"Tap detected at: {tapPos}");

        if (_camera == null)
        {
            SetDebug("ERROR: No camera on this object.");
            return;
        }

        Ray ray = _camera.ScreenPointToRay(tapPos);

        RaycastHit[] hits = Physics.RaycastAll(ray, _maxDistance, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
        {
            SetDebug("NO COLLIDER HIT -> prefab has NO collider / disabled / too far");
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Print first hit info
        var first = hits[0];
        SetDebug($"Hit: {first.collider.name} | layer={LayerMask.LayerToName(first.collider.gameObject.layer)} | dist={first.distance:0.00}");

        // Find collectible
        for (int i = 0; i < hits.Length; i++)
        {
            CollectOnTap collectible = hits[i].collider.GetComponentInParent<CollectOnTap>();
            if (collectible == null)
                continue;

            SetDebug($"COLLECT: {hits[i].collider.name}");
            collectible.TryCollect();
            return;
        }

        SetDebug("Hit colliders, but NONE has CollectOnTap in parent.");
    }

    private static bool TryGetTapPosition(out Vector2 pos)
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                pos = touch.position.ReadValue();
                return true;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pos = Mouse.current.position.ReadValue();
            return true;
        }
#endif

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }
#endif

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == UnityEngine.TouchPhase.Began)
            {
                pos = t.position;
                return true;
            }
        }

        pos = default;
        return false;
    }

    private void SetDebug(string msg)
    {
        if (_debugText != null)
            _debugText.text = msg;
    }
}
