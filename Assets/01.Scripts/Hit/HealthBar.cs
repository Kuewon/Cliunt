using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    [Header("Entity Settings")]
    [SerializeField] private EntityType entityType;

    [Header("Components")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fillImage;

    [Header("Color Settings")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    private float mediumHealthThreshold = 0.7f;
    private float lowHealthThreshold = 0.4f;

    private float maxHealth;
    private float currentHealth;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (fillImage == null)
        {
            Debug.LogError("HealthBar: Fill Image가 할당되지 않았습니다!");
        }

        if (fillImage != null)
        {
            fillImage.material = new Material(Shader.Find("UI/Default"));
            fillImage.color = Color.white;
        }
        
        // 엔티티 타입에 따라 위치 설정
        if (rectTransform != null)
        {
            SetPositionByEntityType();
        }
    }

    private void SetPositionByEntityType()
    {
        switch (entityType)
        {
            case EntityType.Player:
                rectTransform.anchoredPosition = new Vector2(-7f, -140f);
                break;
            case EntityType.Enemy:
                rectTransform.anchoredPosition = new Vector2(100f, -120f);
                break;
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

        if (fillAmount > mediumHealthThreshold)
        {
            fillImage.color = highHealthColor;
        }
        else if (fillAmount > lowHealthThreshold)
        {
            fillImage.color = mediumHealthColor;
        }
        else
        {
            fillImage.color = lowHealthColor;
        }
    }
}