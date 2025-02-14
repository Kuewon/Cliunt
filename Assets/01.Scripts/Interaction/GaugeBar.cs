using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GaugeBar : MonoBehaviour
{
    [SerializeField] private Slider gaugeSlider;  // Slider UI
    [SerializeField] private float increaseAmount = 10f;  // Hit당 증가량
    [SerializeField] private float maxGauge = 200f;  // 최대치
    [SerializeField] private float currentGauge = 0f;  // 현재 게이지 수치

    [Header("자동 감소 설정")]
    [SerializeField] private float decreaseRate = 0.5f;  // 0.5%씩 감소
    private float decreaseInterval = 0.1f;  // 0.1초 간격

    private void Start()
    {
        // 시작할 때 게이지 초기화
        gaugeSlider.minValue = 0f;
        gaugeSlider.maxValue = maxGauge;
        gaugeSlider.value = currentGauge;  // 초기 게이지 값 설정

        // 자동 감소 시작
        StartCoroutine(AutoDecreaseGauge());
    }

    // Hit이 발생했을 때 호출될 함수
    public void IncreaseGauge()
    {
        currentGauge = Mathf.Min(currentGauge + increaseAmount, maxGauge);
        gaugeSlider.value = currentGauge;

        if (currentGauge >= maxGauge)
        {
            Debug.Log("게이지가 가득 찼습니다!");
        }
    }

    // 자동 감소 코루틴
    private IEnumerator AutoDecreaseGauge()
    {
        while (true)
        {
            yield return new WaitForSeconds(decreaseInterval);

            if (currentGauge > 0)
            {
                // maxGauge의 0.5%만큼 감소
                float decreaseValue = maxGauge * (decreaseRate / 100f);
                currentGauge = Mathf.Max(0, currentGauge - decreaseValue);
                gaugeSlider.value = currentGauge;
            }
        }
    }

    // 현재 게이지 값을 가져오는 프로퍼티
    public float CurrentGauge => currentGauge;
}