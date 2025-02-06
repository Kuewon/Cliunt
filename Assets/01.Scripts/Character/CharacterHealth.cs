using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    private float maxHealth;
    private float currentHealth;
    private HitEffect hitEffect;
    private HealthBar healthBar;
    private bool isInitialized = false;

    private void Awake()
    {
        // 히트 이펙트 컴포넌트 초기화
        hitEffect = GetComponent<HitEffect>();
        if (hitEffect == null)
        {
            hitEffect = gameObject.AddComponent<HitEffect>();
        }
    }

    private void Start()
    {
        // 구글 시트 데이터 로드 완료 이벤트 구독
        GoogleSheetsManager.OnDataLoadComplete += InitializeHealth;
        
        // GameData가 이미 초기화되어 있는지 확인
        object baseHealthValue = GameData.Instance.GetValue("PlayerStats", 0, "baseHealth");
        if (baseHealthValue != null)
        {
            // 데이터가 이미 있다면 바로 초기화
            InitializeHealth();
        }
        else
        {
            // 데이터가 없다면 임시로 기본값 설정
            SetDefaultHealth();
            Debug.Log("⚠️ 구글 시트 데이터 로드 대기 중... 임시로 기본 체력 설정");
        }
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= InitializeHealth;
    }

    private void InitializeHealth()
    {
        if (isInitialized) return; // 중복 초기화 방지

        object baseHealthValue = GameData.Instance.GetValue("PlayerStats", 0, "baseHealth");
        if (baseHealthValue != null)
        {
            // 문자열을 float로 변환할 때 예외 처리 추가
            if (float.TryParse(baseHealthValue.ToString(), out float newHealth))
            {
                maxHealth = newHealth;
                currentHealth = maxHealth;
                
                // 이전 체력바가 있다면 제거하고 새로 생성
                if (healthBar != null)
                {
                    Destroy(healthBar.gameObject);
                }
                
                healthBar = HealthBar.Create(transform, maxHealth);
                isInitialized = true;
                
                Debug.Log($"✅ PlayerStats에서 체력 데이터를 성공적으로 로드했습니다. 기본 체력: {maxHealth}");
            }
            else
            {
                Debug.LogError($"❌ 체력 값 변환 실패: {baseHealthValue}");
                SetDefaultHealth();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerStats에서 baseHealth 값을 찾을 수 없습니다. 기본값 유지");
        }
    }

    private void SetDefaultHealth()
    {
        if (isInitialized) return; // 이미 초기화되었다면 무시

        maxHealth = 100f;
        currentHealth = maxHealth;
        
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
        
        healthBar = HealthBar.Create(transform, maxHealth);
        Debug.Log("ℹ️ 기본 체력 값으로 초기화: " + maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ 체력 시스템이 아직 초기화되지 않았습니다!");
            return;
        }

        if (healthBar == null)
        {
            healthBar = HealthBar.Create(transform, maxHealth);
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        healthBar.UpdateHealth(currentHealth);

        if (hitEffect != null)
        {
            hitEffect.PlayHitEffect();
        }

        DamagePopup.Create(transform.position, damage);
        Debug.Log($"🔸 데미지 {damage} 적용됨. 현재 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("💀 캐릭터 사망!");
        // 게임오버 처리나 리스폰 로직을 여기에 추가
    }
}