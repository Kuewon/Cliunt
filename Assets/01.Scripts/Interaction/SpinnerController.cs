using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    private float lastClickTime = 0f;
    private float clickCooldown = 0.2f;

    [Header("⚡ 회전 속도 설정")]
    private float maxSpeed = 2000f;
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
    [SerializeField] private float shortDragBoost = 1.05f;

    [Header("📊 UI 속도 및 회전 수 표시")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rotationText;

    [Header("🎯 총알 이미지 설정")]
    private Image[] bullettImages;
    private int[] bullettStates;
    private EquipmentManager equipmentManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        InitializeBullettImages();
        
        equipmentManager = FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogWarning("⚠️ EquipmentManager를 찾을 수 없습니다!");
        }

        bullettStates = new int[6] { -1, -1, -1, -1, -1, -1 };
    }

    private void Start()
    {
        UpdateMaxSpeedFromEquippedCylinder();
        UpdateBullettFromEquipment();
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
                Debug.Log($"🎯 Bullett_{i} 이미지 컴포넌트 찾음");
            }
            else
            {
                Debug.LogWarning($"⚠️ Bullett_{i}를 찾을 수 없습니다!");
            }
        }
    }

    public void UpdateBullettFromEquipment()
    {
        if (equipmentManager != null)
        {
            int equippedBulletIndex = equipmentManager.GetEquippedBulletIndex();
        
            // 모든 슬롯에 동일한 총알 장착
            for (int i = 0; i < bullettStates.Length; i++)
            {
                bullettStates[i] = equippedBulletIndex;
            }

            InitializeBullettImages();
            UpdateBullettVisibility(); // UI 업데이트를 위해 추가
        
            Debug.Log($"🎯 모든 슬롯에 총알 {equippedBulletIndex} 장착 완료");
        }
    }

    private void UpdateBullettVisibility()
    {
        for (int i = 0; i < 6; i++)
        {
            if (bullettImages[i] != null)
            {
                bullettImages[i].enabled = (bullettStates[i] != -1);
                Debug.Log($"🎯 Bullett_{i} 상태: {(bullettImages[i].enabled ? "켜짐" : "꺼짐")}");
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
            Debug.LogWarning($"⚠️ 잘못된 총알 인덱스: {index}");
            return false;
        }
        return bullettStates[index] != -1;
    }

    public void OnBulletEquipped()
    {
        UpdateBullettFromEquipment();
        UpdateBullettVisibility();
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