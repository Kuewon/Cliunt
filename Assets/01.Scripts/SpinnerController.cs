using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    [SerializeField] private float edgeThickness = 0.7f;
    [SerializeField] private float dragThreshold = 3f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField]
    [Range(0.9f, 0.9999f)]
    private float dampingRate = 0.995f;

    private float angleThresholdMultiplier = 3f;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private Vector2 spinnerCenter;
    private Vector2 lastMousePosition;
    private float targetAngularVelocity;
    private bool isDragging;

    private CoolingBar coolingBar;  // CoolingBar 참조 추가

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
        spinnerCenter = transform.position;
        coolingBar = FindObjectOfType<CoolingBar>();  // CoolingBar 찾기

        rb.angularDrag = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        Vector2 inputPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
            CheckInputClick(inputPosition);
        else if (Input.GetMouseButton(0) && isDragging)
            HandleDrag(inputPosition);
        else if (Input.GetMouseButtonUp(0))
            isDragging = false;

        ApplyRotation();
    }

    private void CheckInputClick(Vector2 inputPosition)
    {
        // 게이지가 잠겨있으면 드래그 불가
        if (coolingBar != null && coolingBar.IsLocked)
        {
            return;
        }

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

        targetAngularVelocity = anglePerSecond < threshold ?
            -absAngle * 60f :
            -absAngle * (120f + Mathf.Clamp01((anglePerSecond - threshold) / threshold) * 240f);

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
            if (Mathf.Abs(rb.angularVelocity) < 100f)
            {
                rb.angularVelocity *= dampingRate * 0.95f;
            }
            else
            {
                rb.angularVelocity *= dampingRate;
            }

            if (Mathf.Abs(rb.angularVelocity) < 40f)
            {
                rb.angularVelocity = 0f;
            }
        }
    }
}