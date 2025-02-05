using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    [SerializeField] private float edgeThickness = 0.3f;
    [SerializeField] private float dragThreshold = 3f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float angleThresholdMultiplier = 3f;
    [SerializeField] private float dampingRate = 0.999995f;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    private Vector2 spinnerCenter;
    private Vector2 lastMousePosition;
    private float targetAngularVelocity;
    private bool isDragging;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
        spinnerCenter = transform.position;
        rb.angularDrag = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

#if UNITY_ANDROID || UNITY_IOS
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
#endif
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

        // 회전 방향을 항상 오른쪽으로 만듦
        float absAngle = Mathf.Abs(angle);

        float anglePerSecond = absAngle / Time.deltaTime;
        float threshold = dragThreshold * angleThresholdMultiplier;

#if UNITY_ANDROID || UNITY_IOS
        targetAngularVelocity = anglePerSecond < threshold ?
            -absAngle * 10f :
            -absAngle * (20f + Mathf.Clamp01((anglePerSecond - threshold) / threshold) * 40f);
#else
        targetAngularVelocity = anglePerSecond < threshold ?
            -absAngle * 40f :
            -absAngle * (80f + Mathf.Clamp01((anglePerSecond - threshold) / threshold) * 160f);
#endif
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
            rb.angularVelocity *= Mathf.Pow(dampingRate, Time.deltaTime * 10f);
            if (Mathf.Abs(rb.angularVelocity) < 0.0001f)
                rb.angularVelocity = 0f;
        }
    }
}