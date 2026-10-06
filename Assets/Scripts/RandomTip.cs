using UnityEngine;
using TMPro;

public class RandomTip : MonoBehaviour
{
    [Tooltip("Assign the GameObject to enable when showing a tip.")]
    public GameObject targetObject;

    [Tooltip("Assign the TextMeshProUGUI component to display the tip.")]
    public TextMeshProUGUI tipText;

    [Tooltip("Add tip strings here.")]
    public string[] tips = new string[] { "sunny", "cloudy", "rainy" };

    // Call this to show a random tip (enables targetObject and sets text)
    public void ShowRandomTip()
    {
        if (targetObject == null || tipText == null || tips == null || tips.Length == 0)
        {
            Debug.LogWarning("RandomTip: Missing assignment or empty tips array.");
            return;
        }

        int index = Random.Range(0, tips.Length);
        tipText.text = tips[index];
        targetObject.SetActive(true);
    }

    // Optional: Hide the target object
    public void HideTip()
    {
        if (targetObject != null) targetObject.SetActive(false);
    }
}
