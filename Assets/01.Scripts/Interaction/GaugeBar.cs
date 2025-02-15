using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GaugeBar : MonoBehaviour
{
    [SerializeField] private Slider gaugeSlider;
    [SerializeField] private float increaseAmount = 10f;
    [SerializeField] private float maxGauge = 200f;
    private float targetGauge = 0f;
    private float currentGauge = 0f;

    [Header("자동 감소 설정")]
    [SerializeField] private float decreaseRate = 0.5f;
    private float decreaseInterval = 0.1f;
    private bool isMaxGauge = false;
    [SerializeField] private float smoothSpeed = 5f;

    private void Start()
    {
        gaugeSlider.minValue = 0f;
        gaugeSlider.maxValue = maxGauge;
        gaugeSlider.value = currentGauge;

        StartCoroutine(AutoDecreaseGauge());
    }

    private void Update()
    {
        gaugeSlider.value = Mathf.Lerp(gaugeSlider.value, targetGauge, Time.deltaTime * smoothSpeed);
    }

    public void IncreaseGauge()
    {
        if (isMaxGauge) return;

        targetGauge = Mathf.Min(targetGauge + increaseAmount, maxGauge);

        if (targetGauge >= maxGauge)
        {
            isMaxGauge = true;
        }
    }

    private IEnumerator AutoDecreaseGauge()
    {
        while (true)
        {
            yield return new WaitForSeconds(decreaseInterval);

            if (isMaxGauge && targetGauge > 0)
            {
                float decreaseValue = maxGauge * (decreaseRate / 100f);
                targetGauge = Mathf.Max(0, targetGauge - decreaseValue);

                if (targetGauge <= 0)
                {
                    isMaxGauge = false;
                }
            }
        }
    }

    public float CurrentGauge => targetGauge;
    public float GetMaxGauge()
    {
        return maxGauge;
    }
}