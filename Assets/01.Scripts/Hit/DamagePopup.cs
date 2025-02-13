using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private int fontSize = 40;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private Material fontMaterial;

    [Header("Animation Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private float growthDuration = 0.5f;
    [SerializeField] private float growthSpeed = 0.3f;
    [SerializeField] private float shrinkSpeed = 1f;
    [SerializeField] private float fadeSpeed = 2f;

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private float disappearTimer;
    private Color textColor;
    private Vector2 moveVector;
    private bool isCritical;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textMesh = GetComponent<TextMeshProUGUI>();

        // 스크립트의 폰트 설정을 강제 적용
        ApplyFontSettings();
    }
    private void ApplyFontSettings()
    {
        if (textMesh != null)
        {
            // 폰트 에셋 적용
            if (fontAsset != null)
            {
                textMesh.font = fontAsset;
            }

            // 폰트 머티리얼 적용
            if (fontMaterial != null)
            {
                textMesh.fontMaterial = fontMaterial;
            }

            // 기타 텍스트 설정
            textMesh.fontSize = fontSize;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableWordWrapping = false;
        }
    }

    public void Setup(Vector3 worldPosition, float damageAmount, bool isCritical = false)
    {
        this.isCritical = isCritical;

        // 캔버스 참조 가져오기
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || Camera.main == null) return;

        // 월드 좌표를 UI 좌표로 변환
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                Camera.main.WorldToScreenPoint(worldPosition),
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out Vector2 localPoint))
        {
            localPoint.y -= 200f;
            rectTransform.anchoredPosition = localPoint;
        }

        // 텍스트 설정
        textMesh.text = damageAmount.ToString("F1");
    
        if (isCritical)
        {
            textMesh.color = criticalColor;
            transform.localScale *= 1.2f;
            textMesh.fontSize = fontSize * 1.2f;
        }
        else
        {
            textMesh.color = normalColor;
            textMesh.fontSize = fontSize;
        }

        textColor = textMesh.color;
        disappearTimer = lifetime;

        float speedMultiplier = isCritical ? 1.5f : 1f;
        moveVector = new Vector2(Random.Range(-0.5f, 0.5f), 0.3f) * moveSpeed * speedMultiplier;
    }

    private void Update()
    {
        rectTransform.anchoredPosition += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > lifetime - growthDuration)
        {
            float scaleMultiplier = isCritical ? growthSpeed * 1.5f : growthSpeed;
            transform.localScale += Vector3.one * Time.deltaTime * scaleMultiplier;
        }
        else
        {
            transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            textColor.a -= Time.deltaTime * fadeSpeed;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}