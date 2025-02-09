using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public event Action OnEnemyDeath;

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
        
        // WaveManager에 이 적을 등록
        WaveManager.Instance?.RegisterEnemy(this);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.Setup(maxHealth);
        }
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator PushBack()
    {
        isBeingPushed = true;

        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player")?.transform.position ?? transform.position;
        Vector3 pushDirection = (transform.position - playerPosition).normalized;
        Vector3 targetPosition = transform.position + pushDirection * criticalHitPushForce;

        float elapsed = 0f;
        float pushDuration = 0.2f;
        Vector3 startPosition = transform.position;

        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pushDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        isBeingPushed = false;
    }

    private void Die()
    {
        OnEnemyDeath?.Invoke();
        var GM = FindObjectOfType<GameManager>();
        var WM = FindObjectOfType<WaveManager>();
        var gold = gameObject.GetComponent<EnemyMoveController>()._enemyDropGold;
        int temp = Mathf.CeilToInt(gold * WM.enemyDropGoldMultiplier);
        GM.OnUpdateGold(temp);
        Destroy(gameObject);
    }
}