using UnityEngine;
using TMPro; // 🔹 TextMeshPro 사용

public class SpinnerController : MonoBehaviour
{
    private RectTransform rectTransform;
    private float currentSpinSpeed;
    public bool isDragging;
    private Vector2 lastMousePosition;
    private float previousForce = 0f;
    private float dragStartTime;
    private float totalRotation = 0f;
    private int rotationCount = 0;

    private float lastClickTime = 0f; // 🔹 마지막 클릭 시간 저장
    private float clickCooldown = 0.2f; // 🔹 최소 클릭 간격 (0.2초)

    [Header("⚡ 회전 속도 설정")]
    [SerializeField] private float maxSpeed = 2000f;
    [SerializeField] private float accelerationMultiplier = 2.0f;

    [Header("🎯 힘 조절 설정")]
    [SerializeField] private float minForce = 10f;
    [SerializeField] private float maxForce = 2000f;
    [SerializeField] private float maxAcceleration = 600f;
    [SerializeField] private float powerCurve = 2.0f;

    [Header("📏 해상도 조정")]
    [SerializeField] private float baseScreenWidth = 1080f;
    [SerializeField] private float baseScreenHeight = 1920f;

    [Header("🛑 감속 설정")]
    [SerializeField] private float dampingRate = 0.99f;
    [SerializeField] private float fixedDeceleration = 15f;
    [SerializeField] private float spinStopThreshold = 10f;
    [SerializeField] private float quickStopFactor = 2f;

    [Header("⏳ 조작 시간 설정")]
    [SerializeField] private float shortDragThreshold = 0.2f;
    [SerializeField] private float shortDragBoost = 1.05f; // 🔹 Boost 값 낮춤

    [Header("📊 UI 속도 및 회전 수 표시")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rotationText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        ApplyRotation();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (speedText != null)
            speedText.text = "Speed: " + Mathf.Round(currentSpinSpeed).ToString();

        if (rotationText != null)
            rotationText.text = "Rotations: " + rotationCount;
    }

    public void CheckInputClick(Vector2 inputPosition)
    {
        if (Time.time - lastClickTime < clickCooldown) return; // 🔹 너무 빠른 클릭 무시

        lastClickTime = Time.time;
        isDragging = true;
        dragStartTime = Time.time;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, inputPosition, null, out Vector2 localPos);
        lastMousePosition = localPos / 1000f;
    }

    public void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;

        float widthRatio = Screen.width / baseScreenWidth;
        float heightRatio = Screen.height / baseScreenHeight;
        float scaleFactor = Mathf.Lerp(0.5f, 1f, widthRatio); // 🔹 모바일 감도 조절

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, currentPosition, null, out Vector2 localPos);
        Vector2 newMousePosition = localPos / 1000f;
        Vector2 delta = newMousePosition - lastMousePosition;

        if (delta.magnitude > 0)
        {
            float normalizedDelta = delta.magnitude / (Screen.height * 0.5f);
            float distanceFactor = Mathf.Pow(Mathf.Clamp(normalizedDelta, 0, 1f), powerCurve) * heightRatio * scaleFactor;
            float rawForce = minForce + (maxForce - minForce) * Mathf.Pow(distanceFactor, 1.5f);

            rawForce = Mathf.Clamp(rawForce, minForce, maxForce * 0.75f);

            float deltaSpeed = Mathf.Lerp(previousForce, rawForce, 0.1f);
            deltaSpeed = Mathf.Clamp(deltaSpeed, 0, maxAcceleration);

            float speedFactor = Mathf.Lerp(1f, 0.5f, currentSpinSpeed / maxSpeed);
            deltaSpeed *= speedFactor;

            currentSpinSpeed += deltaSpeed * accelerationMultiplier;
            currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, 0, maxSpeed);

            previousForce = deltaSpeed * 0.8f;
        }

        lastMousePosition = newMousePosition;
    }

    public void OnDragEnd()
    {
        isDragging = false;
        float dragDuration = Time.time - dragStartTime;

        if (dragDuration < shortDragThreshold)
        {
            currentSpinSpeed = Mathf.Min(currentSpinSpeed * shortDragBoost, maxSpeed * 0.7f); // 🔹 급격한 가속 방지
            previousForce *= 0.9f; // 🔹 속도 누적 제한
        }
        else
        {
            previousForce *= 0.6f;
        }
    }

    private void ApplyRotation()
    {
        if (currentSpinSpeed > spinStopThreshold)
        {
            float appliedDamping = isDragging ? dampingRate : 0.985f;
            currentSpinSpeed *= appliedDamping;

            currentSpinSpeed = Mathf.Max(0, currentSpinSpeed - (fixedDeceleration * Time.deltaTime * (isDragging ? 1f : 0.5f)));
        }
        else
        {
            currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, 0f, Time.deltaTime * quickStopFactor);
        }

        float rotationAmount = -Mathf.Abs(currentSpinSpeed) * Time.deltaTime;
        rectTransform.Rotate(0, 0, rotationAmount);

        totalRotation += Mathf.Abs(rotationAmount);
        rotationCount = Mathf.FloorToInt(totalRotation / 360f);
    }
}
