using UnityEngine;
using TMPro;

public class TechnicalMetricsUI : MonoBehaviour
{
    public TextMeshProUGUI tctText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI retryText;
    public TextMeshProUGUI efficiencyText;

    public void UpdateMetrics(float tct, float accuracy, float errorRate, int retry, float efficiency)
    {
        tctText.text = $"TCT: {tct:F2}s";
        accuracyText.text = $"Accuracy: {accuracy:F1}%";
        errorText.text = $"Error Rate: {errorRate:F2}";
        retryText.text = $"Retry Count: {retry}";
        efficiencyText.text = $"Task Efficiency: {efficiency:F2}";
    }
}
