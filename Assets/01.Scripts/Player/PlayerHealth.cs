using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private DamagePopup damagePopupPrefab;  // 데미지 팝업 프리팹

    private float maxHealth;
    private float currentHealth;
    private HitEffect hitEffect;
    private bool isInitialized = false;
    private Transform canvasTransform;  // TopIngame 캔버스 캐싱용

    private void Awake()
    {
        hitEffect = GetComponent<HitEffect>();
        if (hitEffect == null)
        {
            hitEffect = gameObject.AddComponent<HitEffect>();
        }

        // TopIngame 캔버스 찾아서 캐싱
        GameObject canvasObj = GameObject.FindWithTag("TopIngame");
        if (canvasObj != null)
        {
            canvasTransform = canvasObj.transform;
        }
        else
        {
            Debug.LogError("TopIngame 태그를 가진 캔버스를 찾을 수 없습니다!");
        }
    }

    private void Start()
    {
        GoogleSheetsManager.OnDataLoadComplete += InitializeHealth;

        object baseHealthValue = GameData.Instance.GetValue("PlayerStats", 0, "baseHealth");
        if (baseHealthValue != null)
        {
            InitializeHealth();
        }
        else
        {
            SetDefaultHealth();
            Debug.Log("⚠️ 구글 시트 데이터 로드 대기 중... 임시로 기본 체력 설정");
        }
    }

    private void InitializeHealth()
    {
        if (isInitialized) return;

        object baseHealthValue = GameData.Instance.GetValue("PlayerStats", 0, "baseHealth");
        if (baseHealthValue != null && float.TryParse(baseHealthValue.ToString(), out float newHealth))
        {
            maxHealth = newHealth;
            currentHealth = maxHealth;

            if (healthBar != null)
            {
                healthBar.Setup(maxHealth);
                isInitialized = true;
                Debug.Log($"✅ PlayerStats에서 체력 데이터를 성공적으로 로드했습니다. 기본 체력: {maxHealth}");
            }
            else
            {
                Debug.LogError("HealthBar가 할당되지 않았습니다. Inspector에서 할당해주세요.");
            }
        }
        else
        {
            Debug.LogError($"❌ 체력 값 변환 실패: {baseHealthValue}");
            SetDefaultHealth();
        }
    }

    private void SetDefaultHealth()
    {
        if (isInitialized) return;

        maxHealth = 100f;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.Setup(maxHealth);
            Debug.Log("ℹ️ 기본 체력 값으로 초기화: " + maxHealth);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ 체력 시스템이 아직 초기화되지 않았습니다!");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth);
        }

        if (hitEffect != null)
        {
            hitEffect.PlayHitEffect();
        }

        // 데미지 팝업 생성
        if (damagePopupPrefab != null && canvasTransform != null)
        {
            Vector3 worldPosition = transform.position;
            DamagePopup popup = Instantiate(damagePopupPrefab, canvasTransform);
            popup.Setup(worldPosition, damage);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        var gold = FindObjectOfType<GameManager>().totalGold;
        PlayerPrefs.SetFloat("USER_GOLD", gold);
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= InitializeHealth;
    }
}