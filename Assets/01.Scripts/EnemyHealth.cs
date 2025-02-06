using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Hit Effects")]
    [SerializeField] private Color normalHitColor = Color.white;
    [SerializeField] private Color criticalHitColor = Color.red;
    [SerializeField] private float criticalHitPushForce = 0.2f;
    
    private float currentHealth;
    private HitEffect hitEffect;
    private HealthBar healthBar;
    private Vector3 originalPosition;
    private bool isBeingPushed = false;

    private void Start()
    {
        currentHealth = maxHealth;
        originalPosition = transform.position;
        
        hitEffect = GetComponent<HitEffect>();
        if (hitEffect == null)
        {
            hitEffect = gameObject.AddComponent<HitEffect>();
        }

        healthBar = HealthBar.Create(transform, maxHealth);
    }

    public void TakeDamage(float damage, bool isCritical = false)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        healthBar.UpdateHealth(currentHealth);

        if (hitEffect != null)
        {
            if (isCritical)
            {
                hitEffect.PlayCriticalHitEffect();
                if (!isBeingPushed)
                {
                    StartCoroutine(PushBack());
                }
            }
            else
            {
                hitEffect.PlayHitEffect();
            }
        }

        DamagePopup.Create(transform.position + Vector3.up * 0.5f, damage, isCritical);

        string hitType = isCritical ? "크리티컬" : "일반";
        Debug.Log($"적이 {hitType} 공격으로 {damage} 데미지를 받았습니다. 남은 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator PushBack()
    {
        isBeingPushed = true;
        
        // 플레이어 방향으로 밀려나기
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player")?.transform.position ?? transform.position;
        Vector3 pushDirection = (transform.position - playerPosition).normalized;
        Vector3 targetPosition = transform.position + pushDirection * criticalHitPushForce;
        
        float elapsed = 0f;
        float pushDuration = 0.2f;
        Vector3 startPosition = transform.position;
        
        // 밀려나는 동작
        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pushDuration;
            t = 1f - Mathf.Pow(1f - t, 3f); // 이징 함수
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        isBeingPushed = false;
    }

    private void Die()
    {
        if (currentHealth <= -maxHealth * 0.2f)
        {
            Debug.Log("💥 치명적인 일격으로 적이 처치되었습니다!");
        }
        else
        {
            Debug.Log("적이 사망했습니다!");
        }
        
        Destroy(gameObject);
    }
}