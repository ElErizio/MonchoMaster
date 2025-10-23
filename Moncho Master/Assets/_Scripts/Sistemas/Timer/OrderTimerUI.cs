using UnityEngine;
using TMPro;

public class OrderTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    public void UpdateTimerText(float remainingTime)
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(remainingTime);
        timerText.text = $"{seconds}";

        timerText.color = remainingTime <= 10f ? warningColor : normalColor;
    }
}
