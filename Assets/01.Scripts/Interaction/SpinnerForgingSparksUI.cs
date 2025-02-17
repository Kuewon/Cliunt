using UnityEngine;

public class SpinnerForgingSparksUI : MonoBehaviour
{
    public RectTransform spinnerTransform; // UI 스피너의 RectTransform
    public ParticleSystem forgingSparks;  // 쇠 튀기는 불꽃 효과

    public float speedThreshold = 50f; // 회전 속도 기준 (이 값 이하로 떨어지면 효과 발생)
    private float lastRotation; // 이전 프레임의 회전값
    private float currentSpeed; // 현재 속도
    private bool hasPlayedSparks = false; // 효과가 한 번만 실행되도록 체크

    void Update()
    {
        // 현재 회전 값 가져오기 (z축 회전)
        float currentRotation = spinnerTransform.eulerAngles.z;

        // 속도 계산 (현재 회전 값 - 이전 프레임의 회전 값)
        currentSpeed = Mathf.Abs(currentRotation - lastRotation) / Time.deltaTime;

        // 속도가 낮아질 때 한 번만 불꽃 효과 발생
        if (currentSpeed < speedThreshold && !hasPlayedSparks)
        {
            forgingSparks.transform.position = spinnerTransform.position; // 현재 UI 위치에서 불꽃 생성
            forgingSparks.Play();
            hasPlayedSparks = true; // 중복 실행 방지
        }

        // 속도가 다시 높아지면 효과를 다시 실행할 수 있도록 초기화
        if (currentSpeed > speedThreshold)
        {
            hasPlayedSparks = false;
        }

        // 현재 회전 값을 저장하여 다음 프레임에서 비교할 수 있도록 함
        lastRotation = currentRotation;
    }
}