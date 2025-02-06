using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectMask2D))]
public class CoolingBar : MonoBehaviour
{
    private RectMask2D fillBarMask;
    private RectTransform rectTransform;
    private float maxGauge;
    private float currentGauge = 0f;
    private float gaugeHeight;

    private float decreaseInterval;
    private float decreaseRate;
    private float timer = 0f;

    private bool isLocked = false;
    public bool IsLocked => isLocked;

    private void Awake()
    {
        fillBarMask = GetComponent<RectMask2D>();
        rectTransform = GetComponent<RectTransform>();
        gaugeHeight = rectTransform.rect.height;
        fillBarMask.padding = new Vector4(0, 0, 0, gaugeHeight);

        LoadStatsFromSheet();
    }

    private void LoadStatsFromSheet()
    {
        var stats = GameData.Instance.GetRow("RevolverStats", 0);
        if (stats != null)
        {
            // 구글 시트에서 기본값 로드
            maxGauge = ConvertToFloat(stats["baseCoolingGauge"]);
            decreaseRate = ConvertToFloat(stats["decreaseRate"]);
            decreaseInterval = ConvertToFloat(stats["decreaseInterval"]);

            Debug.Log($"✅ Revolver Stats 로드 완료: maxGauge={maxGauge}, decreaseRate={decreaseRate}, decreaseInterval={decreaseInterval}");
        }
        else
        {
            Debug.LogError("❌ RevolverStats 데이터를 불러오는데 실패했습니다. 기본값을 사용합니다.");
            // 기본값 설정
            maxGauge = 100f;
            decreaseRate = 0.005f;
            decreaseInterval = 0.1f;
        }
    }

    private float ConvertToFloat(object value)
    {
        if (value == null) return 0f;

        if (value is float floatValue)
            return floatValue;

        if (value is int intValue)
            return (float)intValue;

        if (float.TryParse(value.ToString(), out float result))
            return result;

        Debug.LogError($"❌ 값 변환 실패: {value}");
        return 0f;
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