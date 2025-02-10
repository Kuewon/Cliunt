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

    // 캐릭터용 헬스바 생성
    public static HealthBar CreatePlayerHealthBar(Transform parent, float maxHealth)
    {
        GameObject canvasObj = CreateCanvas("PlayerHealthBarCanvas");
        canvasObj.transform.SetParent(parent);
        canvasObj.transform.localPosition = new Vector3(0, -2.5f, 0); // 캐릭터 아래에 위치

        SetupCanvasProperties(canvasObj, new Vector2(0.5f, 0.08f));
        CreateBackground(canvasObj);
        Image fillImage = CreateFillBar(canvasObj);

        return SetupHealthBarComponent(canvasObj, fillImage, maxHealth);
    }

    // 적용 헬스바 생성
    public static HealthBar CreateEnemyHealthBar(Transform parent, float maxHealth)
    {
        GameObject canvasObj = CreateCanvas("EnemyHealthBarCanvas");
        canvasObj.transform.SetParent(parent);
        canvasObj.transform.localPosition = new Vector3(0, -0.3f, 0); // 적 아래에 위치

        SetupCanvasProperties(canvasObj, new Vector2(0.5f, 0.08f));
        CreateBackground(canvasObj);
        Image fillImage = CreateFillBar(canvasObj);

        return SetupHealthBarComponent(canvasObj, fillImage, maxHealth);
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        return canvasObj;
    }

    private static void SetupCanvasProperties(GameObject canvasObj, Vector2 size)
    {
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = size;
        canvasRect.localScale = Vector3.one;
    }

    private static void CreateBackground(GameObject parent)
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent.transform);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        SetupRectTransform(bgRect);
    }

    private static Image CreateFillBar(GameObject parent)
    {
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(parent.transform);

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        SetupRectTransform(fillRect);

        return fillImage;
    }

    private static void SetupRectTransform(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
    }

    private static HealthBar SetupHealthBarComponent(GameObject canvasObj, Image fillImage, float maxHealth)
    {
        HealthBar healthBar = canvasObj.AddComponent<HealthBar>();
        healthBar.fillImage = fillImage;
        healthBar.Setup(maxHealth);
        return healthBar;
    }
}