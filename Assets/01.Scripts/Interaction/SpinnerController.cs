using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    private RectTransform rectTransform; // 피젯 스피너의 회전을 적용할 RectTransform
    private float currentSpinSpeed; // 현재 회전 속도
    public bool isDragging; // 사용자가 드래그 중인지 여부
    private Vector2 lastMousePosition; // 마지막 입력 위치 (로컬 좌표)
    private float previousForce = 0f; // 이전 프레임에서 사용된 힘 저장

    [Header("🌀 실시간 속도 디버깅 (읽기 전용)")]
    [SerializeField] private float debugSpinSpeed; // 현재 회전 속도를 인스펙터에서 확인

    [Header("⚡ 회전 속도 설정")]
    [SerializeField] private float maxSpeed = 3000f; // 최대 회전 속도
    [SerializeField] private float accelerationMultiplier = 5.0f; // 드래그 중 가속 조절
    [SerializeField] private float speedSmoothing = 0.5f; // 속도를 부드럽게 적용하는 정도

    [Header("🎯 힘 조절 설정")]
    [SerializeField] private float minForce = 10f; // 최소 힘 (작은 이동 시 적용)
    [SerializeField] private float maxForce = 2000f; // 최대 힘 (큰 이동 시 적용)
    [SerializeField] private float maxAcceleration = 1000f; // 한 번의 드래그에서 최대 가속량 제한
    [SerializeField] private float accelerationDamping = 0.3f; // 속도 증가 억제 계수 (높을수록 억제)
    [SerializeField] private float powerCurve = 3.0f; // 힘이 증가하는 곡선 (낮을수록 선형, 높을수록 작은 이동 시 힘이 적게 적용됨)

    [Header("📏 해상도 조정")]
    [SerializeField] private float baseScreenWidth = 1080f; // 기준이 되는 화면 가로 크기
    [SerializeField] private float baseScreenHeight = 1920f; // 기준이 되는 화면 세로 크기

    [Header("🛑 감속 설정")]
    [SerializeField] private float dampingRate = 0.995f; // 감속 계수 (1에 가까울수록 감속이 느림)
    [SerializeField] private float fixedDeceleration = 10f; // 초당 일정 속도 감소
    [SerializeField] private float spinStopThreshold = 10f; // 회전이 멈추는 기준 속도
    [SerializeField] private float quickStopFactor = 1.5f; // 빠르게 멈출 때 감속 가중치

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        debugSpinSpeed = currentSpinSpeed;
        ApplyRotation();
    }

    public void CheckInputClick(Vector2 inputPosition)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, inputPosition, null, out Vector2 localPos);
        lastMousePosition = localPos / 1000f;
    }

    public void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;

        // 현재 기기의 해상도를 기준으로 비율 계산
        float widthRatio = Screen.width / baseScreenWidth;
        float heightRatio = Screen.height / baseScreenHeight;

        // 현재 마우스 위치를 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, currentPosition, null, out Vector2 localPos);
        Vector2 newMousePosition = localPos / 1000f;
        Vector2 delta = newMousePosition - lastMousePosition;

        if (delta.magnitude > 0)
        {
            // ✅ 이동 거리를 해상도에 따라 정규화 (높이를 기준으로 비율 보정)
            float normalizedDelta = delta.magnitude / (Screen.height * 0.5f);

            // ✅ 거리 계수 조정 (해상도 영향을 줄이기 위해 `heightRatio` 사용)
            float distanceFactor = Mathf.Pow(Mathf.Clamp(normalizedDelta, 0, 1f), powerCurve) * heightRatio;

            // ✅ 최소 힘을 `minForce`로 설정하여 작은 이동일 때 과도한 힘 방지
            float rawForce = minForce + (maxForce - minForce) * Mathf.Pow(distanceFactor, 1.5f);

            // ✅ 이전 힘과 새 힘을 조화롭게 결합하여 부드러운 증가
            float deltaSpeed = Mathf.Lerp(previousForce, rawForce, 0.3f);

            // ✅ 가속이 너무 빠르게 증가하는 것을 방지 (최대 가속량 제한)
            deltaSpeed = Mathf.Clamp(deltaSpeed, 0, maxAcceleration);

            // ✅ 속도 증가 억제 (현재 속도가 높을수록 가속을 줄임)
            float dampingFactor = Mathf.Lerp(1f, accelerationDamping, currentSpinSpeed / maxSpeed);
            deltaSpeed *= dampingFactor;

            // ✅ 기존 속도에 힘을 부드럽게 더함
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, currentSpinSpeed + deltaSpeed * accelerationMultiplier, speedSmoothing);

            // ✅ 최대 속도 제한
            currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, 0, maxSpeed);

            // ✅ 이번 프레임에서 적용한 힘을 저장
            previousForce = deltaSpeed;

            // 🔍 디버그 로그 추가
            //Debug.Log($"[HandleDrag] 이동 거리: {delta.magnitude}, 정규화 거리: {normalizedDelta}, 적용된 힘: {deltaSpeed}, 현재 속도: {currentSpinSpeed}, 거리 계수: {distanceFactor}, 해상도 비율: {heightRatio}");
        }
        else
        {
            //Debug.Log($"[HandleDrag] 마우스 이동 없음, 속도 변화 없음.");
        }

        lastMousePosition = newMousePosition;
    }

    public void OnDragEnd()
    {
        isDragging = false;
        
        // ✅ 기존 힘을 90% 유지하여 갑작스러운 감속 방지
        previousForce *= 0.9f;

        //Debug.Log($"[OnDragEnd] 마우스 놓음, 현재 속도: {currentSpinSpeed}, 이전 힘 유지: {previousForce}");
    }

    private void ApplyRotation()
    {
        if (currentSpinSpeed > spinStopThreshold)
        {
            // ✅ 손을 뗀 후에도 감속을 천천히 적용하도록 수정
            float appliedDamping = isDragging ? dampingRate : Mathf.Lerp(dampingRate, 0.995f, Time.deltaTime * quickStopFactor);

            // 감속 적용
            currentSpinSpeed *= appliedDamping;
            currentSpinSpeed = Mathf.Max(0, currentSpinSpeed - (fixedDeceleration * Time.deltaTime * (isDragging ? 1f : 0.5f))); 
        }
        else
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, Time.deltaTime * quickStopFactor);
        }

        rectTransform.Rotate(0, 0, -Mathf.Abs(currentSpinSpeed) * Time.deltaTime);
    }
}
