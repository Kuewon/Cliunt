using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public event Action OnEnemyDeath;

    [Header("Components")]
    [SerializeField] private HealthBar healthBar;  // Inspector에서 할당
    [SerializeField] private DamagePopup damagePopupPrefab;

    [Header("Health Settings")]
    private float maxHealth = 100f;

    [Header("Hit Effects")]
    [SerializeField] private Color normalHitColor = Color.white;
    [SerializeField] private Color criticalHitColor = Color.red;
    [SerializeField] private float criticalHitPushForce = 0.2f;

    private float currentHealth;
    private HitEffect hitEffect;
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

        if (healthBar != null)
        {
            healthBar.Setup(maxHealth);
        }
        else
        {
            Debug.LogError("HealthBar가 할당되지 않았습니다. Inspector에서 할당해주세요.");
        }

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
        
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth);
        }

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

        if (damagePopupPrefab != null)
        {
            // 월드 좌표를 스크린 좌표로 변환
            Vector3 worldPosition = transform.position + Vector3.up * 0.5f;
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            // TopIngame 캔버스 찾기
            GameObject canvasObj = GameObject.FindWithTag("TopIngame");
            if (canvasObj != null)
            {
                // 프리팹 생성 및 설정
                DamagePopup popup = Instantiate(damagePopupPrefab, canvasObj.transform);
                popup.Setup(screenPosition, damage, isCritical);
            }
        }

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