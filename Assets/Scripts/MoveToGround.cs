using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class MoveToGround : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>();
    [Tooltip("Lerp speed for vertical movement. 0 = instant.")]
    public float lerpSpeed = 10f;
    [Tooltip("Minimum plane area (m²) to consider as ground.")]
    public float minPlaneArea = 0.1f;

    ARPlaneManager planeManager;
    float groundY = float.NegativeInfinity;

    void Awake() => planeManager = GetComponent<ARPlaneManager>();

    [System.Obsolete]
    void OnEnable() => planeManager.planesChanged += OnPlanesChanged;
    [System.Obsolete]
    void OnDisable() => planeManager.planesChanged -= OnPlanesChanged;

    [System.Obsolete]
    void OnPlanesChanged(ARPlanesChangedEventArgs args) => UpdateGroundY();

    void Start() => UpdateGroundY();

    void UpdateGroundY()
    {
        float lowest = float.PositiveInfinity;
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp) continue;
            // optional: require enough area
            if (plane.size.x * plane.size.y < minPlaneArea) continue;
            // plane.transform.position.y is a reliable world Y for the plane
            float y = plane.transform.position.y;
            if (y < lowest) lowest = y;
        }

        if (lowest != float.PositiveInfinity) groundY = lowest;
    }

    void LateUpdate()
    {
        if (groundY == float.NegativeInfinity) return; // no ground yet
        foreach (var t in targets)
        {
            if (t == null) continue;
            Vector3 targetPos = t.position;
            float newY = groundY;
            if (lerpSpeed > 0f) targetPos.y = Mathf.Lerp(targetPos.y, newY, Time.deltaTime * lerpSpeed);
            else targetPos.y = newY;
            t.position = targetPos;
        }
    }
}
