using TMPro;
using UnityEngine;

public class TunnelStatsUI : MonoBehaviour
{
    public TextMeshProUGUI horizontalDistanceText;
    public TextMeshProUGUI maxDepthText;

    public void UpdateStats(float horizontalDistance, float maxDepth)
    {
        horizontalDistanceText.text = $"Distance: {horizontalDistance:F1}m";
        maxDepthText.text = $"Max Depth: {maxDepth:F1}m";
    }

    public void Show(bool show)
    {
        horizontalDistanceText.gameObject.SetActive(show);
        maxDepthText.gameObject.SetActive(show);
    }
}
