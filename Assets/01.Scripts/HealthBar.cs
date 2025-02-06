using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private Camera mainCamera;
    private float maxHealth;
    private float currentHealth;

    public void Setup(float maxHp)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        UpdateHealthBar();
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // 항상 카메라를 향하도록
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
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

        // 체력에 따라 색상 변경
        if (fillAmount > 0.6f)
            fillImage.color = Color.green;
        else if (fillAmount > 0.3f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;
    }

    public static HealthBar Create(Transform parent, float maxHealth)
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(parent);
        canvasObj.transform.localPosition = new Vector3(0, -0.3f, 0); // 캐릭터 위에 위치

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        // 캔버스 크기 조정
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(0.5f, 0.08f);
        canvasRect.localScale = new Vector3(1f, 1f, 1f);

        // 배경 이미지
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.localPosition = Vector3.zero;

        // 체력바 이미지
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.localPosition = Vector3.zero;

        // HealthBar 컴포넌트 추가
        HealthBar healthBar = canvasObj.AddComponent<HealthBar>();
        healthBar.fillImage = fillImage;
        healthBar.Setup(maxHealth);

        return healthBar;
    }
}