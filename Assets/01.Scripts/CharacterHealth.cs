using UnityEngine;
using UnityEngine.Serialization;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private HitEffect hitEffect;
    private HealthBar healthBar;

    private void Start()
    {
        currentHealth = maxHealth;
        hitEffect = GetComponent<HitEffect>();
        if (hitEffect == null)
        {
            hitEffect = gameObject.AddComponent<HitEffect>();
        }

        // 체력바 생성
        healthBar = HealthBar.Create(transform, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // 체력바 업데이트
        healthBar.UpdateHealth(currentHealth);

        if (hitEffect != null)
        {
            hitEffect.PlayHitEffect();
        }

        DamagePopup.Create(transform.position, damage);

        Debug.Log($"플레이어가 {damage} 데미지를 받았습니다. 남은 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");
        // 게임오버 처리나 리스폰 로직을 여기에 추가
    }
}