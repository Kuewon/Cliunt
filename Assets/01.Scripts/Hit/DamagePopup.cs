using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private int fontSize = 40;
    [SerializeField] private float characterSize = 0.1f;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Animation Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float growthDuration = 0.5f;
    [SerializeField] private float growthSpeed = 0.3f;
    [SerializeField] private float shrinkSpeed = 1f;
    [SerializeField] private float fadeSpeed = 2f;
    
    private TextMesh textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;
    private static Camera mainCamera;
    private bool isCritical;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        InitializeTextMesh();
    }

    private void InitializeTextMesh()
    {
        textMesh = gameObject.AddComponent<TextMesh>();

        // TextMesh 컴포넌트 추가 및 설정
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = characterSize;

        // MeshRenderer 설정
        if (TryGetComponent(out MeshRenderer meshRenderer)) 
        {
            meshRenderer.sortingLayerName = "UI";
            meshRenderer.sortingOrder = 100;
        }
    }

    public static DamagePopup Create(Vector3 position, float damageAmount, bool isCritical = false)
    {
        GameObject damagePopupObj = new GameObject("DamagePopup");
        DamagePopup damagePopup = damagePopupObj.AddComponent<DamagePopup>();
        damagePopup.Setup(position, damageAmount, isCritical);
        return damagePopup;
    }

    private void Setup(Vector3 position, float damageAmount, bool isCritical)
    {
        this.isCritical = isCritical;
        transform.position = position + new Vector3(0, 0.5f, 0);
        
        // 텍스트 설정
        textMesh.text = damageAmount.ToString("F1");
        
        // 크리티컬이면 더 크게 표시
        if (isCritical)
        {
            textMesh.color = criticalColor;
            transform.localScale *= 1.2f;
            fontSize = (int)(fontSize * 1.2f);
        }
        else
        {
            textMesh.color = normalColor;
        }

        textColor = textMesh.color;
        disappearTimer = lifetime;
        
        // 크리티컬은 더 빠르게 위로 올라감
        float speedMultiplier = isCritical ? 1.5f : 1f;
        moveVector = new Vector3(Random.Range(-1f, 1f), 1) * moveSpeed * speedMultiplier;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }

        // 이동 처리
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        // 크기 변화
        if (disappearTimer > lifetime - growthDuration)
        {
            float scaleMultiplier = isCritical ? growthSpeed * 1.5f : growthSpeed;
            transform.localScale += Vector3.one * Time.deltaTime * scaleMultiplier;
        }
        else
        {
            transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
        }

        // 페이드 아웃
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

    private void OnValidate()
    {
        // 에디터에서 값이 변경될 때 텍스트 메시 업데이트
        if (textMesh != null)
        {
            textMesh.fontSize = fontSize;
            //textMesh.characterSize = characterSize;
        }
    }
}