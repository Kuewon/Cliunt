using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    [SerializeField] private float edgeThickness = 0.7f;
    [SerializeField] private float dragThreshold = 3f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField][Range(0.9f, 0.9999f)] private float dampingRate = 0.995f;

    private float angleThresholdMultiplier = 3f;
    private float baseSpinSpeed;
    private float baseMaxSpinSpeed;
    private float spinMinVelocity;
    private float spinStopThreshold = 40f;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private Vector2 spinnerCenter;
    private Vector2 lastMousePosition;
    private float targetAngularVelocity;
    private bool isDragging;

    private CoolingBar coolingBar;
    private bool wasLocked = false;
    private float originalRotation;
    private bool isJittering = false;
    private float jitterAngle = 5f;
    private float jitterDuration = 0.1f;
    private float jitterTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
        spinnerCenter = transform.position;
        coolingBar = FindObjectOfType<CoolingBar>();

        rb.angularDrag = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 🔴 하드코딩된 값으로 설정
        baseSpinSpeed = 0f;
        baseMaxSpinSpeed = 5f;
        spinMinVelocity = 100f;

        Debug.Log($"✅ SpinnerController 초기화 완료: baseSpinSpeed={baseSpinSpeed}, baseMaxSpinSpeed={baseMaxSpinSpeed}, spinMinVelocity={spinMinVelocity}");
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

        if (Input.GetMouseButtonDown(0))
            CheckInputClick(inputPosition);
        else if (Input.GetMouseButton(0) && isDragging)
            HandleDrag(inputPosition);
        else if (Input.GetMouseButtonUp(0))
            isDragging = false;

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

    private void CheckInputClick(Vector2 inputPosition)
    {
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
}
