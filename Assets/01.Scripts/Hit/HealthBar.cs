using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform barTransform;

    [Header("Position Settings")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0, 30f);

    [Header("Color Settings")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float mediumHealthThreshold = 0.6f;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    private float maxHealth;
    private float currentHealth;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (barTransform == null) barTransform = GetComponent<RectTransform>();
        
        // 부모 오브젝트의 위치를 기준으로 상대 위치 설정
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = positionOffset;
        }
    }

    public void Setup(float maxHp)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        UpdateHealthBar();
    }

    public void UpdateHealth(float health)
    {
        currentHealth = health;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        float fillAmount = currentHealth / maxHealth;
        fillImage.fillAmount = Mathf.Clamp01(fillAmount);

        // 체력에 따른 색상 변경
        if (fillAmount > mediumHealthThreshold)
            fillImage.color = highHealthColor;
        else if (fillAmount > lowHealthThreshold)
            fillImage.color = mediumHealthColor;
        else
            fillImage.color = lowHealthColor;
    }
}