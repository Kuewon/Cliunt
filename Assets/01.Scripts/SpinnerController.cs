using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    // 인스펙터에서 조절 가능한 값들
    [SerializeField] private float edgeThickness = 0.7f;        // 스피너 가장자리를 감지하는 두께
    [SerializeField] private float dragThreshold = 3f;          // 천천히/빠르게 드래그를 구분하는 기준값
    [SerializeField] private float smoothSpeed = 10f;           // 회전이 얼마나 부드럽게 변할지 결정
    [SerializeField]
    [Range(0.9f, 0.9999f)]                    // 슬라이더로 조절 가능한 범위 설정
    private float dampingRate = 0.995f;                         // 감속 비율 (값이 작을수록 빨리 멈춤)

    // 내부 변수들
    private float angleThresholdMultiplier = 3f;                // 각도 임계값 배수 (드래그 속도 판정에 사용)
    private Rigidbody2D rb;                                     // 물리 효과를 위한 컴포넌트
    private CircleCollider2D circleCollider;                    // 클릭 감지를 위한 콜라이더
    private Camera mainCamera;                                  // 마우스/터치 위치 계산용
    private Vector2 spinnerCenter;                              // 스피너의 중심점
    private Vector2 lastMousePosition;                          // 이전 프레임의 마우스 위치
    private float targetAngularVelocity;                        // 목표 회전 속도
    private bool isDragging;                                    // 현재 드래그 중인지 여부

    private void Start()
    {
        // 필요한 컴포넌트들 가져오기
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
        spinnerCenter = transform.position;

        // 물리 설정 초기화
        rb.angularDrag = 0;                                     // 자동 감속 제거
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 부드러운 움직임 설정
    }

    private void Update()
    {
        // 현재 마우스/터치 위치 가져오기
        Vector2 inputPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 입력 상태에 따른 처리
        if (Input.GetMouseButtonDown(0))                        // 클릭 시작
            CheckInputClick(inputPosition);
        else if (Input.GetMouseButton(0) && isDragging)         // 드래그 중
            HandleDrag(inputPosition);
        else if (Input.GetMouseButtonUp(0))                     // 클릭 종료
            isDragging = false;

        ApplyRotation();                                        // 회전 적용
    }

    private void CheckInputClick(Vector2 inputPosition)
    {
        // 클릭한 위치가 스피너 가장자리에 있는지 확인
        float radius = circleCollider.radius * transform.localScale.x;
        if (Vector2.Distance(inputPosition, transform.position) <= radius &&
            Vector2.Distance(inputPosition, transform.position) >= radius * (1 - edgeThickness))
        {
            isDragging = true;
            lastMousePosition = inputPosition;
            spinnerCenter = transform.position;
        }
    }

    private void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;

        // 드래그 방향과 각도 계산
        Vector2 lastVector = lastMousePosition - spinnerCenter;
        Vector2 currentVector = currentPosition - spinnerCenter;
        float angle = Vector2.SignedAngle(lastVector, currentVector);

        // 회전 속도 계산
        float absAngle = Mathf.Abs(angle);
        float anglePerSecond = absAngle / Time.deltaTime;       // 초당 회전 각도
        float threshold = dragThreshold * angleThresholdMultiplier;

        // 회전력 계산 - 드래그 속도에 따라 다른 힘 적용
        targetAngularVelocity = anglePerSecond < threshold ?
            -absAngle * 60f :                                   // 천천히 돌릴 때 (기본 힘)
            -absAngle * (120f + Mathf.Clamp01((anglePerSecond - threshold) / threshold) * 240f);
        // 빠르게 돌릴 때 (기본 힘 + 추가 가속)

        // 연속 회전시 추가 가속도 적용
        if (!isDragging)
        {
            // 현재 속도의 20%만큼 추가 가속
            targetAngularVelocity += Mathf.Sign(targetAngularVelocity) * Mathf.Abs(rb.angularVelocity) * 0.2f;
        }

        lastMousePosition = currentPosition;
    }

    private void ApplyRotation()
    {
        if (isDragging)
        {
            // 드래그 중일 때는 목표 속도로 부드럽게 전환
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, targetAngularVelocity, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // 낮은 속도에서는 더 강한 감속 적용
            if (Mathf.Abs(rb.angularVelocity) < 100f)          // 100 이하의 속도에서
            {
                rb.angularVelocity *= dampingRate * 0.95f;      // 추가 감속 적용 (5% 더 강하게)
            }
            else
            {
                rb.angularVelocity *= dampingRate;              // 기본 감속만 적용
            }

            // 매우 낮은 속도에서는 완전히 정지
            if (Mathf.Abs(rb.angularVelocity) < 40f)           // 40 이하면 정지
            {
                rb.angularVelocity = 0f;
            }
        }
    }
}