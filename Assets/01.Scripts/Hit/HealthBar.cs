using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image backgroundImage;  // 테두리/배경 이미지
    [SerializeField] private Image fillImage;        // 체력바 이미지
    
    [Header("Position Settings")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0, 30f);

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
        
        // fillImage 할당 확인
        if (fillImage == null)
        {
            Debug.LogError("HealthBar: Fill Image가 할당되지 않았습니다!");
        }

        // 초기 색상 설정
        if (fillImage != null)
        {
            fillImage.material = new Material(Shader.Find("UI/Default"));
            fillImage.color = Color.white;
        }
        
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