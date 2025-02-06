using UnityEngine;
using UnityEngine.UI;

public class CoolingBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Image fillBar;
    [SerializeField] private RectMask2D fillBarMask;

    [Header("Gauge Settings")]
    [SerializeField] private float baseCoolingGauge = 100f;        // �⺻ �� ������ �ִ밪
    [SerializeField] private float baseCoolingRate = 0.0005f;      // �⺻ �� ȸ���� (0.05%)
    [SerializeField] private float decreaseRate = 0.005f;          // ������ ������ (0.5%)
    [SerializeField] private float decreaseInterval = 0.1f;        // ������ ���� ���� (0.1��)

    private SpinnerGameManager gameManager;
    private float currentGauge = 0f;
    private bool isSpinnerLocked = false;
    private float timer = 0f;
    private float gaugeHeight;

    private void Awake()
    {
        if (backgroundBar == null || fillBar == null)
        {
            Debug.LogError("Required UI components are missing on CoolingBar!");
            enabled = false;
            return;
        }

        if (fillBarMask == null)
        {
            fillBarMask = fillBar.GetComponent<RectMask2D>();
        }

        gaugeHeight = fillBar.rectTransform.rect.height;
    }

    private void Start()
    {
        gameManager = FindObjectOfType<SpinnerGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("SpinnerGameManager not found in the scene!");
        }

        InitializeUI();
        UpdateGaugeVisual();
    }

    private void InitializeUI()
    {
        fillBar.type = Image.Type.Simple;
        fillBar.fillCenter = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= decreaseInterval)
        {
            DecreaseGauge();
            timer = 0f;
        }
    }

    // ���� �Ŵ������� ȣ���� �޼���
    public bool IncrementGauge(float amount)
    {
        if (isSpinnerLocked)
        {
            return false;
        }

        currentGauge += amount;

        if (currentGauge >= baseCoolingGauge)
        {
            currentGauge = baseCoolingGauge;
            isSpinnerLocked = true;
            // ���� �Ŵ������� ���ǳ� ��� ���� �˸�
            if (gameManager != null)
            {
                gameManager.OnSpinnerLocked(true);
            }
        }

        UpdateGaugeVisual();
        return true;
    }

    private void DecreaseGauge()
    {
        float baseCoolingAmount = baseCoolingGauge * baseCoolingRate;
        float decreaseAmount = baseCoolingGauge * decreaseRate;
        float totalChange = decreaseAmount - baseCoolingAmount;

        currentGauge = Mathf.Max(0f, currentGauge - totalChange);

        if (currentGauge <= 0f)
        {
            currentGauge = 0f;
            if (isSpinnerLocked)
            {
                isSpinnerLocked = false;
                // ���� �Ŵ������� ���ǳ� ��� ���� ���� �˸�
                if (gameManager != null)
                {
                    gameManager.OnSpinnerLocked(false);
                }
            }
        }

        UpdateGaugeVisual();
    }

    private void UpdateGaugeVisual()
    {
        if (fillBar != null && fillBarMask != null)
        {
            float fillRatio = currentGauge / baseCoolingGauge;
            float maskHeight = (1f - fillRatio) * gaugeHeight;
            fillBarMask.padding = new Vector4(0, 0, 0, maskHeight);
        }
    }

    public float GetGaugePercentage()
    {
        return (currentGauge / baseCoolingGauge) * 100f;
    }

    public bool IsLocked()
    {
        return isSpinnerLocked;
    }
}