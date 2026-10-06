using System.Collections.Generic;
using UnityEngine;

public class TempDisabler : MonoBehaviour
{
    [Tooltip("Objects to toggle")]
    public List<GameObject> targets = new List<GameObject>();

    // Call this to toggle each target's active state.
    public void ToggleTargets()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var go = targets[i];
            if (go == null) continue;
            go.SetActive(!go.activeSelf);
        }
    }

    // Optional: toggle a single target by index (safe).
    public void ToggleTargetAtIndex(int index)
    {
        if (index < 0 || index >= targets.Count) return;
        var go = targets[index];
        if (go == null) return;
        go.SetActive(!go.activeSelf);
    }
}
