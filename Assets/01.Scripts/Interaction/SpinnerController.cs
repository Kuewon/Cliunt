using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    private RectTransform rectTransform; // 스피너의 회전을 적용할 RectTransform
    private float currentSpinSpeed; // 현재 회전 속도
    public bool isDragging; // 사용자가 드래그 중인지 여부
    private Vector2 lastMousePosition; // 마지막 입력 위치 (로컬 좌표)
    private float previousForce = 0f; // 이전 프레임에서 사용된 힘 저장
    private float dragStartTime; // 드래그 시작 시간 저장

    [Header("⚡ 회전 속도 설정")]
    [SerializeField] private float maxSpeed = 3000f; // 최대 회전 속도
    [SerializeField] private float accelerationMultiplier = 6.0f; // 가속 배율
    [SerializeField] private float speedSmoothing = 0.5f; // 속도를 부드럽게 적용하는 정도

    [Header("🎯 힘 조절 설정")]
    [SerializeField] private float minForce = 10f; // 최소 힘 (작은 이동 시 적용)
    [SerializeField] private float maxForce = 2000f; // 최대 힘 (큰 이동 시 적용)
    [SerializeField] private float maxAcceleration = 1000f; // 한 번의 드래그에서 최대 가속량 제한
    [SerializeField] private float accelerationDamping = 0.3f; // 속도 증가 억제 계수 (높을수록 억제)
    [SerializeField] private float powerCurve = 2.0f; // 힘이 증가하는 곡선 (낮을수록 선형, 높을수록 작은 이동 시 힘이 적게 적용됨)

    [Header("📏 해상도 조정")]
    [SerializeField] private float baseScreenWidth = 1080f; // 기준이 되는 화면 가로 크기
    [SerializeField] private float baseScreenHeight = 1920f; // 기준이 되는 화면 세로 크기

    [Header("🛑 감속 설정")]
    [SerializeField] private float dampingRate = 0.99f; // 감속 계수 (1에 가까울수록 감속이 느림)
    [SerializeField] private float fixedDeceleration = 20f; // 초당 일정 속도 감소
    [SerializeField] private float spinStopThreshold = 10f; // 회전이 멈추는 기준 속도
    [SerializeField] private float quickStopFactor = 2f; // 빠르게 멈출 때 감속 가중치

    [Header("⏳ 조작 시간 설정")]
    [SerializeField] private float shortDragThreshold = 0.2f; // "짧은 드래그"로 판단할 최소 시간(초)
    [SerializeField] private float shortDragBoost = 1.5f; // "짧은 드래그" 시 속도를 추가로 높이는 값

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        ApplyRotation();
    }

    // 사용자가 터치(마우스 클릭)했을 때 실행
    public void CheckInputClick(Vector2 inputPosition)
    {
        isDragging = true;
        dragStartTime = Time.time; // 드래그 시작 시간 기록
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, inputPosition, null, out Vector2 localPos);
        lastMousePosition = localPos / 1000f;
    }

    // 사용자가 드래그하는 동안 실행
    public void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;

        float widthRatio = Screen.width / baseScreenWidth;
        float heightRatio = Screen.height / baseScreenHeight;

        // 현재 마우스 위치를 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, currentPosition, null, out Vector2 localPos);
        Vector2 newMousePosition = localPos / 1000f;
        Vector2 delta = newMousePosition - lastMousePosition;

        if (delta.magnitude > 0)
        {
            // 이동 거리를 해상도에 따라 정규화
            float normalizedDelta = delta.magnitude / (Screen.height * 0.5f);
            float distanceFactor = Mathf.Pow(Mathf.Clamp(normalizedDelta, 0, 1f), powerCurve) * heightRatio;
            float rawForce = minForce + (maxForce - minForce) * Mathf.Pow(distanceFactor, 1.5f);

            // 부드러운 가속 적용
            float deltaSpeed = Mathf.Lerp(previousForce, rawForce, 0.3f);
            deltaSpeed = Mathf.Clamp(deltaSpeed, 0, maxAcceleration);

            // 속도가 높을수록 가속이 덜 들어가도록 제한 (스택 과다 방지)
            float speedFactor = Mathf.Lerp(1f, 0.5f, currentSpinSpeed / maxSpeed);
            deltaSpeed *= speedFactor;

            // 기존 속도에 힘을 부드럽게 더함
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, currentSpinSpeed + deltaSpeed * accelerationMultiplier, speedSmoothing);
            currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, 0, maxSpeed);

            // 마지막 힘을 저장하여 손을 떼어도 남아 있도록 함
            previousForce = deltaSpeed * 0.8f;
        }

        lastMousePosition = newMousePosition;
    }

    // 사용자가 터치(마우스 클릭)를 해제했을 때 실행
    public void OnDragEnd()
    {
        isDragging = false;
        float dragDuration = Time.time - dragStartTime; // 조작 시간 계산

        if (dragDuration < shortDragThreshold)
        {
            // "짧은 드래그" → 속도 증가
            currentSpinSpeed *= shortDragBoost; // 추가적인 속도 증가
            previousForce *= 1.2f; // 속도 스택을 더 쌓음
        }
        else
        {
            // "길게 누르고 돌린 경우" → 감속 강하게 적용
            previousForce *= 0.6f; // 손을 떼면 속도를 더 빠르게 줄임
        }
    }

    // 스피너의 회전을 적용하는 함수
    private void ApplyRotation()
    {
        if (currentSpinSpeed > spinStopThreshold)
        {
            // 손을 뗀 후에도 감속을 천천히 적용
            float appliedDamping = isDragging ? dampingRate : Mathf.Lerp(dampingRate, 0.99f, Time.deltaTime * quickStopFactor);
            currentSpinSpeed *= appliedDamping;

            // 감속 적용
            currentSpinSpeed = Mathf.Max(0, currentSpinSpeed - (fixedDeceleration * Time.deltaTime * (isDragging ? 1f : 0.5f)));
        }
        else
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, Time.deltaTime * quickStopFactor);
        }

        // 실제 회전 적용 (Z축 기준 회전)
        rectTransform.Rotate(0, 0, -Mathf.Abs(currentSpinSpeed) * Time.deltaTime);
    }
}
