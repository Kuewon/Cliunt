using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpinnerController : MonoBehaviour
{
    private RectTransform rectTransform; // 스피너의 RectTransform 컴포넌트
    private float currentSpinSpeed; // 현재 회전 속도
    public bool isDragging; // 사용자가 드래그 중인지 여부
    private Vector2 lastMousePosition; // 마지막 마우스 위치
    private float previousForce = 0f; // 이전에 적용된 힘
    private float dragStartTime; // 드래그 시작 시간
    private float totalRotation = 0f; // 총 회전량
    private int rotationCount = 0; // 총 회전 횟수

    private float lastClickTime = 0f; // 마지막 클릭 시간
    private float clickCooldown = 0.2f; // 클릭 간격 제한 (쿨다운)

    [Header("⚡ 회전 속도 설정")]
    private float maxSpeed = 2000f; // 최대 회전 속도
    [SerializeField] private float accelerationMultiplier = 2.0f; // 가속 계수

    [Header("🎯 힘 조절 설정")]
    [SerializeField] private float minForce = 10f; // 최소 힘
    [SerializeField] private float maxForce = 2000f; // 최대 힘
    [SerializeField] private float maxAcceleration = 600f; // 최대 가속도
    [SerializeField] private float powerCurve = 2.0f; // 힘 증가 곡선 조절값

    [Header("📏 해상도 조정")]
    [SerializeField] private float baseScreenWidth = 1080f; // 기준 화면 너비
    [SerializeField] private float baseScreenHeight = 1920f; // 기준 화면 높이

    [Header("🛑 감속 설정")]
    [SerializeField] private float dampingRate = 0.99f; // 감속 비율
    [SerializeField] private float fixedDeceleration = 15f; // 고정 감속량
    [SerializeField] private float spinStopThreshold = 10f; // 회전 멈춤 기준 속도
    [SerializeField] private float quickStopFactor = 2f; // 빠른 정지 계수

    [Header("⏳ 조작 시간 설정")]
    [SerializeField] private float shortDragThreshold = 0.2f; // 짧은 드래그 판정 시간
    [SerializeField] private float shortDragBoost = 1.05f; // 짧은 드래그 시 속도 증가율

    [Header("📊 UI 속도 및 회전 수 표시")]
    public TextMeshProUGUI speedText; // 속도 표시 UI
    public TextMeshProUGUI rotationText; // 회전 횟수 표시 UI

    [Header("🎯 총알 이미지 설정")]
    private Image[] bullettImages;
    private int[] bullettStates = new int[] { 19, 0, 0, 8, -1, -1 }; // 하드코딩된 초기값

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        InitializeBullettImages();
    }

    private void Start()
    {
        UpdateMaxSpeedFromEquippedCylinder();
        UpdateBullettVisibility();
    }

    private void Update()
    {
        ApplyRotation();
        UpdateUI();
    }

    private void InitializeBullettImages()
    {
        bullettImages = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            Transform bullettTransform = transform.Find($"Bullett_{i}");
            if (bullettTransform != null)
            {
                bullettImages[i] = bullettTransform.GetComponent<Image>();
            }
        }
    }

    private void UpdateBullettVisibility()
    {
        for (int i = 0; i < 6; i++)
        {
            if (bullettImages[i] != null)
            {
                bullettImages[i].enabled = (bullettStates[i] != -1);
            }
        }
    }

    public void UpdateBullettStates(int[] newStates)
    {
        if (newStates != null && newStates.Length == 6)
        {
            bullettStates = newStates;
            UpdateBullettVisibility();
        }
    }

    public bool IsBullettActive(int index)
    {
        if (index < 0 || index >= bullettStates.Length)
        {
            Debug.LogWarning($"Invalid bullett index: {index}");
            return false;
        }
        return bullettStates[index] != -1;
    }

    private void UpdateMaxSpeedFromEquippedCylinder()
    {
        string sheetName = "Cylinder";
        int equippedIndex = EquipmentManager.Instance.GetEquippedCylinderIndex();

        if (!GameData.Instance.HasRow(sheetName, equippedIndex))
        {
            Debug.LogWarning($"⚠️ {sheetName} 시트에 인덱스 {equippedIndex}가 존재하지 않습니다.");
            return;
        }

        maxSpeed = GameData.Instance.GetFloat(sheetName, equippedIndex, "cylinderMaxSpeed", 250f);
        Debug.Log($"🔄 실린더의 최대 회전 속도가 {maxSpeed}으로 업데이트되었습니다.");
    }

    public void OnCylinderEquipped()
    {
        UpdateMaxSpeedFromEquippedCylinder();
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
        if (Time.time - lastClickTime < clickCooldown) return;

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
        float scaleFactor = Mathf.Lerp(0.5f, 1f, widthRatio);

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
            currentSpinSpeed = Mathf.Min(currentSpinSpeed * shortDragBoost, maxSpeed * 0.7f);
            previousForce *= 0.9f;
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

    public float GetCurrentSpeed()
    {
        return currentSpinSpeed;
    }
}