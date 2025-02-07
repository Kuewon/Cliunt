using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectMask2D))]
public class CoolingBar : MonoBehaviour
{
    private RectMask2D fillBarMask;
    private RectTransform rectTransform;
    private float baseCoolingGauge; // 구글
    private float currentGauge; // 구글
    private float gaugeHeight;
    private float decreaseInterval; // 구글
    private float decreaseRate; // 구글
    private float timer = 0f; // 이건 지금 이해하기 힘듬. 항상0에서 시작하는것만 인지.
    private bool isLocked = false;
    public bool IsLocked => isLocked;

    private void Awake()
    {
        fillBarMask = GetComponent<RectMask2D>();
        rectTransform = GetComponent<RectTransform>();
        gaugeHeight = rectTransform.rect.height;
        fillBarMask.padding = new Vector4(0, 0, 0, gaugeHeight);
    }

    private void Start()
    {
        baseCoolingGauge = (float)GameData.Instance.GetRow("RevolverStats", 0)["baseCoolingGauge"];
        currentGauge = (float)GameData.Instance.GetRow("RevolverStats", 0)["currentGauge"];
        decreaseInterval = (float)GameData.Instance.GetRow("RevolverStats", 0)["decreaseInterval"];
        decreaseRate = (float)GameData.Instance.GetRow("RevolverStats", 0)["decreaseRate"];
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
        float decreaseAmount = baseCoolingGauge * decreaseRate;
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

        currentGauge = Mathf.Min(currentGauge + amount, baseCoolingGauge);

        if (currentGauge >= baseCoolingGauge)
        {
            isLocked = true;
        }

        UpdateGaugeVisual();
    }

    private void UpdateGaugeVisual()
    {
        float fillRatio = currentGauge / baseCoolingGauge;
        float maskHeight = (1f - fillRatio) * gaugeHeight;
        fillBarMask.padding = new Vector4(0, 0, 0, maskHeight);
    }
}
