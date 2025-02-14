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
    private bool isMaxGauge = false;  // 최대 게이지 도달 여부를 체크하는 플래그

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
        // 최대 게이지 상태에서는 더 이상 증가하지 않음
        if (isMaxGauge) return;
        
        currentGauge = Mathf.Min(currentGauge + increaseAmount, maxGauge);
        gaugeSlider.value = currentGauge;

        // 최대 게이지 도달 체크
        if (currentGauge >= maxGauge)
        {
            isMaxGauge = true;
            Debug.Log("게이지가 가득 찼습니다!");
        }
    }

    // 자동 감소 코루틴
    private IEnumerator AutoDecreaseGauge()
    {
        while (true)
        {
            yield return new WaitForSeconds(decreaseInterval);

            // 최대 게이지에 도달한 경우에만 감소
            if (isMaxGauge && currentGauge > 0)
            {
                float decreaseValue = maxGauge * (decreaseRate / 100f);
                currentGauge = Mathf.Max(0, currentGauge - decreaseValue);
                gaugeSlider.value = currentGauge;

                // 게이지가 0이 되면 최대 게이지 플래그 초기화
                if (currentGauge <= 0)
                {
                    isMaxGauge = false;
                }
            }
        }
    }

    // 현재 게이지 값을 가져오는 프로퍼티
    public float CurrentGauge => currentGauge;
}