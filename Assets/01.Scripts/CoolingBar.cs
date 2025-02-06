using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectMask2D))]
public class CoolingBar : MonoBehaviour
{
    private RectMask2D fillBarMask;
    private RectTransform rectTransform;
    private float maxGauge = 100f;
    private float currentGauge = 0f;
    private float gaugeHeight;

    private float decreaseInterval = 0.1f;
    private float decreaseRate = 0.005f;
    private float timer = 0f;

    private bool isLocked = false;
    public bool IsLocked => isLocked;  // 외부에서 상태 확인용 프로퍼티 추가

    private void Awake()
    {
        fillBarMask = GetComponent<RectMask2D>();
        rectTransform = GetComponent<RectTransform>();
        gaugeHeight = rectTransform.rect.height;
        fillBarMask.padding = new Vector4(0, 0, 0, gaugeHeight);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= decreaseInterval)
        {
            DecreaseGauge();
            timer = 0f;
        }
    }

    private void DecreaseGauge()
    {
        float decreaseAmount = maxGauge * decreaseRate;
        currentGauge = Mathf.Max(0f, currentGauge - decreaseAmount);

        if (currentGauge <= 0f && isLocked)
        {
            isLocked = false;
        }

        UpdateGaugeVisual();
    }

    public void IncrementGauge(float amount)
    {
        if (isLocked) return;

        currentGauge = Mathf.Min(currentGauge + amount, maxGauge);

        if (currentGauge >= maxGauge)
        {
            isLocked = true;
        }

        UpdateGaugeVisual();
    }

    private void UpdateGaugeVisual()
    {
        float fillRatio = currentGauge / maxGauge;
        float maskHeight = (1f - fillRatio) * gaugeHeight;
        fillBarMask.padding = new Vector4(0, 0, 0, maskHeight);
    }
}