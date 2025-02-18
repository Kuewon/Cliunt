using UnityEngine;

public class SpeedBasedSparks : MonoBehaviour
{
    [SerializeField] private SpinnerController spinner; // 스피너 컨트롤러 참조
    [SerializeField] private ParticleSystem sparksFX; // 스파크 이펙트

    public float minEmission = 10f; // 최소 방출량
    public float maxEmission = 100f; // 최대 방출량
    public float speedThreshold = 100f; // 스파크 발생 최소 속도

    private ParticleSystem.EmissionModule emissionModule;

    void Start()
    {
        if (sparksFX != null)
        {
            emissionModule = sparksFX.emission;
            emissionModule.enabled = true; // 반드시 활성화 필요
        }
    }

    void Update()
    {
        if (spinner == null || sparksFX == null) return;

        float speed = spinner.GetCurrentSpeed(); // 현재 회전 속도 가져오기

        if (speed > speedThreshold)
        {
            if (!sparksFX.isPlaying)
            {
                sparksFX.Play(); // 속도가 일정 이상이면 파티클 재생
            }

            float normalizedSpeed = Mathf.InverseLerp(speedThreshold, 2000F, speed);
            float emissionRate = Mathf.Lerp(minEmission, maxEmission, normalizedSpeed);
            emissionModule.rateOverTime = emissionRate;
        }
        else
        {
            if (sparksFX.isPlaying)
            {
                sparksFX.Stop(); // 속도가 낮으면 아예 중지
            }
            emissionModule.rateOverTime = 0;
        }
    }
}