using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    public float edgeThickness = 0.7f; // 스피너 외곽부터 터치 범위
    public float dragThreshold = 3f; // 마우스를 얼마나 빠르게 움직여야 스피너 반응하는지(클수록 둔해짐,작을수록 민감해짐)
    public float smoothSpeed = 10f; // 부드럽게 회전하게 해주는 코드
    public float dampingRate = 0.995f; // 마찰력(값이 1에가까울수록 천천히 감소, 0에 가까울수록 빠르게 멈춤)
    public float angleThresholdMultiplier = 3f; // 드래그할 때 회전 속도 계산 시 기준이 되는 값이라는데 이해 못하겠음
    public float baseSpinSpeed; // 구글
    public float baseMaxSpinSpeed; // 구글
    public float spinMinVelocity = 100f; // 스피너가 일정 속도 이하로 떨어지면 감속이 더 강해지는 임계 속도, 낮아질수록 더 빠르게 멈춤
    public float spinStopThreshold = 40f; // 스피너가 완전히 멈추는 기준 속도

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private Vector2 spinnerCenter;
    private Vector2 lastMousePosition;
    private float targetAngularVelocity;
    public bool isDragging;

    private CoolingBar coolingBar;
    private bool wasLocked = false;
    private float originalRotation;
    private bool isJittering = false;
    private float jitterAngle = 5f; // 게이지 다 차고, 움직이려 할 때 깔짝거림 최대 각도
    private float jitterDuration = 0.1f; // 깔짝거림 지속시간
    private float jitterTimer = 0f; // 깔짝거림 효과의 경과 시간을 추적하는 변수라는데 이해 못하겠음.

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
        spinnerCenter = transform.position;
        coolingBar = FindObjectOfType<CoolingBar>();

        rb.angularDrag = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        baseSpinSpeed = (float)GameData.Instance.GetRow("RevolverStats", 0)["baseSpinSpeed"];
        baseMaxSpinSpeed = (float)GameData.Instance.GetRow("RevolverStats", 0)["baseMaxSpinSpeed"];
    }

    private void Update()
    {
        Vector2 inputPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (coolingBar != null && coolingBar.IsLocked && !wasLocked)
        {
            isDragging = false;
            wasLocked = true;
            originalRotation = transform.eulerAngles.z;
            isJittering = false;
        }
        else if (coolingBar != null && !coolingBar.IsLocked)
        {
            wasLocked = false;
        }

        if (coolingBar != null && coolingBar.IsLocked)
        {
            if (Input.GetMouseButtonDown(0) && Mathf.Abs(rb.angularVelocity) < 0.01f)
            {
                float radius = circleCollider.radius * transform.localScale.x;
                if (Vector2.Distance(inputPosition, transform.position) <= radius &&
                    Vector2.Distance(inputPosition, transform.position) >= radius * (1 - edgeThickness))
                {
                    StartJitter();
                }
            }

            if (isJittering)
            {
                HandleJitter();
            }
            else
            {
                ApplyRotation();
            }
            return;
        }

        ApplyRotation();
    }

    private void StartJitter()
    {
        isJittering = true;
        jitterTimer = 0f;
        originalRotation = transform.eulerAngles.z;
    }

    private void HandleJitter()
    {
        jitterTimer += Time.deltaTime;
        if (jitterTimer <= jitterDuration / 2)
        {
            transform.rotation = Quaternion.Euler(0, 0, originalRotation - jitterAngle * (jitterTimer / (jitterDuration / 2)));
        }
        else if (jitterTimer <= jitterDuration)
        {
            float t = (jitterTimer - jitterDuration / 2) / (jitterDuration / 2);
            transform.rotation = Quaternion.Euler(0, 0, originalRotation - jitterAngle * (1 - t));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, originalRotation);
            isJittering = false;
        }
    }

    
    public void CheckInputClick(Vector2 inputPosition)
    {
       // float radius = circleCollider.radius * transform.localScale.x;
       // if (Vector2.Distance(inputPosition, transform.position) <= radius &&
       //     Vector2.Distance(inputPosition, transform.position) >= radius * (1 - edgeThickness))
       // {
            isDragging = true;
            lastMousePosition = inputPosition;
            spinnerCenter = transform.position;
       // }
    }

    
    public void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;

        Vector2 lastVector = lastMousePosition - spinnerCenter;
        Vector2 currentVector = currentPosition - spinnerCenter;
        float angle = Vector2.SignedAngle(lastVector, currentVector);

        float absAngle = Mathf.Abs(angle);
        float anglePerSecond = absAngle / Time.deltaTime;
        float threshold = dragThreshold * angleThresholdMultiplier;

        float minSpeed = 60f;
        float maxSpeed = 360f;

        targetAngularVelocity = anglePerSecond < threshold ?
            -absAngle * minSpeed :
            -absAngle * (minSpeed + Mathf.Clamp01((anglePerSecond - threshold) / threshold) * (maxSpeed - minSpeed));

        if (!isDragging)
        {
            targetAngularVelocity += Mathf.Sign(targetAngularVelocity) * Mathf.Abs(rb.angularVelocity) * 0.2f;
        }

        lastMousePosition = currentPosition;
    }

    private void ApplyRotation()
    {
        if (isDragging)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, targetAngularVelocity, smoothSpeed * Time.deltaTime);
        }
        else
        {
            if (Mathf.Abs(rb.angularVelocity) < spinMinVelocity)
            {
                rb.angularVelocity *= dampingRate * 0.95f;
            }
            else
            {
                rb.angularVelocity *= dampingRate;
            }

            if (Mathf.Abs(rb.angularVelocity) < spinStopThreshold)
            {
                rb.angularVelocity = 0f;
            }
        }
    }

    
    public void OnDragEnd()
    {
        isDragging = false;
    }
}