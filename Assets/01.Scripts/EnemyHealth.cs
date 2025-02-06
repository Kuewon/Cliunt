using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // 피격 효과나 애니메이션 재생 로직을 여기에 추가할 수 있습니다
        Debug.Log($"Enemy took {damage} damage. Current health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 사망 처리 (애니메이션, 파티클 등)
        Debug.Log("Enemy died");
        Destroy(gameObject);
    }
}