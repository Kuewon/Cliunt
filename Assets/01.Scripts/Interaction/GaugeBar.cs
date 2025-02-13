using UnityEngine;
using UnityEngine.UI;

public class GaugeBar : MonoBehaviour
{
    [SerializeField] private Slider gaugeSlider;  // Slider UI
    [SerializeField] private float increaseAmount = 10f;  // Hit당 증가량
    [SerializeField] private float maxGauge = 200f;  // 최대치

    private void Start()
    {
        // 시작할 때 게이지 초기화
        gaugeSlider.minValue = 0f;
        gaugeSlider.maxValue = maxGauge;
        gaugeSlider.value = 0f;
    }

    // Hit이 발생했을 때 호출될 함수
    public void IncreaseGauge()
    {
        float newValue = Mathf.Min(gaugeSlider.value + increaseAmount, maxGauge);
        gaugeSlider.value = newValue;

        if (newValue >= maxGauge)
        {
            Debug.Log("게이지가 가득 찼습니다!");
        }
    }
}