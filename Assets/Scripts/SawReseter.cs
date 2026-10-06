using UnityEngine;

public class SawReseter : MonoBehaviour
{
    [Tooltip("Tag of objects to reset.")]
    public string targetTag = "Saw";

    [Tooltip("New position to set when resetting.")]
    public Vector3 resetPosition = Vector3.zero;

    [Tooltip("New rotation (Euler degrees) to set when resetting.")]
    public Vector3 resetRotationEuler = Vector3.zero;

    [Tooltip("If true, resets all objects with the tag; otherwise resets only the first found.")]
    public bool resetAll = false;

    // Call this from other scripts, UI buttons, or inspector (via context menu)
    [ContextMenu("ResetTargets")]
    public void ResetTargets()
    {
        if (resetAll)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(targetTag);
            foreach (GameObject go in objs)
            {
                ResetTransform(go.transform);
            }
        }
        else
        {
            GameObject go = GameObject.FindWithTag(targetTag);
            if (go != null) ResetTransform(go.transform);
        }
    }

    private void ResetTransform(Transform t)
    {
        t.position = resetPosition;
        t.rotation = Quaternion.Euler(resetRotationEuler);
        // If you want to reset local transform instead, use:
        // t.localPosition = resetPosition;
        // t.localRotation = Quaternion.Euler(resetRotationEuler);
    }
}
