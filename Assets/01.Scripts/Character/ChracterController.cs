using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Base Stats")]
    [SerializeField] private float baseAttackDamage = 10f;
    [SerializeField] private float baseAttackSpeed = 0.2f; // 초당 공격 횟수
    [SerializeField] private float baseAttackRange = 1f;
    [SerializeField] private float baseCriticalChance = 0.1f;
    [SerializeField] private float baseCriticalMultiplier = 1.5f;

    private float attackDamage;
    private float attackInterval;
    private float attackRange;
    private float criticalChance;
    private float criticalMultiplier;

    private bool isInitialized = false;
    private bool isManualAttackPlaying = false;
    private RectTransform rectTransform;

    private const string AUTOATTACK_TRIGGER = "AutoAttack";
    private const string MANUALATTACK_TRIGGER = "ManualAttack";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Gun의 Animator 자동 찾기
        if (gunAnimator == null)
        {
            Transform gunTransform = transform.Find("Gun");
            if (gunTransform != null)
            {
                gunAnimator = gunTransform.GetComponent<Animator>();
            }
        }
        
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        InitializeWithBaseStats();

        GoogleSheetsManager.OnDataLoadComplete += InitializeStats;

        if (GameData.Instance.GetValue("PlayerStats", 0, "baseAttackDamage") != null)
        {
            InitializeStats();
        }
        else
        {
            Debug.Log("⚠️ 구글 시트 데이터 로드 대기 중... 기본 스탯 사용");
        }

        StartCoroutine(AutoAttackRoutine());
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= InitializeStats;
    }

    private void InitializeWithBaseStats()
    {
        attackDamage = baseAttackDamage;
        attackInterval = 1f / baseAttackSpeed;
        attackRange = baseAttackRange;
        criticalChance = baseCriticalChance;
        criticalMultiplier = baseCriticalMultiplier;

        isInitialized = true;
        Debug.Log($"ℹ️ 기본 스탯으로 초기화됨");
    }

    private void InitializeStats()
    {
        var statsData = GameData.Instance.GetRow("PlayerStats", 0);
        if (statsData == null) return;

        // 데이터 파싱 및 적용
        if (TryParseValue(statsData, "baseAttackDamage", out float damage))
            attackDamage = damage;

        if (TryParseValue(statsData, "baseAttackSpeed", out float speed))
            attackInterval = 1f / speed;

        if (TryParseValue(statsData, "attackRange", out float range))
            attackRange = range;

        if (TryParseValue(statsData, "criticalChance", out float critChance))
            criticalChance = critChance;

        if (TryParseValue(statsData, "criticalDamageMultiplier", out float critMulti))
            criticalMultiplier = critMulti;

        Debug.Log($"✅ 스탯 업데이트 완료! 공격력: {attackDamage}, 공격속도: {1f / attackInterval}/s, 사거리: {attackRange}");
    }

    private bool TryParseValue(Dictionary<string, object> data, string key, out float result)
    {
        result = 0f;
        if (!data.ContainsKey(key)) return false;

        string value = data[key]?.ToString();
        if (string.IsNullOrEmpty(value)) return false;

        return float.TryParse(value, out result);
    }

    public void TriggerManualAttack()
    {
        StartCoroutine(ManualAttackRoutine());
    }

    private IEnumerator ManualAttackRoutine()
    {
        isManualAttackPlaying = true;

        // 캐릭터와 총 모두 애니메이션 재생
        animator.ResetTrigger(AUTOATTACK_TRIGGER);
        animator.SetTrigger(MANUALATTACK_TRIGGER);
        
        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger(AUTOATTACK_TRIGGER);
            gunAnimator.SetTrigger(MANUALATTACK_TRIGGER);
        }

        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength * 0.5f);
        PerformAttack("수동");

        yield return new WaitForSeconds(animationLength * 0.5f);

        isManualAttackPlaying = false;
    }

    private IEnumerator AutoAttackRoutine()
    {
        float elapsedTime = 0f;
        float lastAttackTime = 0f;

        while (true)
        {
            if (isInitialized && !isManualAttackPlaying)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime - lastAttackTime >= attackInterval)
                {
                    lastAttackTime = elapsedTime;

                    // 캐릭터와 총 모두 애니메이션 재생
                    animator.SetTrigger(AUTOATTACK_TRIGGER);
                    if (gunAnimator != null)
                    {
                        gunAnimator.SetTrigger(AUTOATTACK_TRIGGER);
                    }

                    float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animationLength);

                    PerformAttack("자동");

                    yield return new WaitForSeconds(animationLength * 0.5f);

                    float remainingInterval = attackInterval - animationLength;
                    if (remainingInterval > 0)
                    {
                        yield return new WaitForSeconds(remainingInterval);
                    }
                }
            }
            yield return null;
        }
    }

    private void PerformAttack(string attackType)
    {
        bool isCritical = Random.value < criticalChance;
        float finalDamage = CalculateDamage(isCritical);

        // UI에서의 적 감지
        var enemies = FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in enemies)
        {
            if (IsEnemyInRange(enemy.gameObject))
            {
                enemy.TakeDamage(finalDamage, isCritical);
            }
        }
    }
    
    private bool IsEnemyInRange(GameObject enemy)
    {
        RectTransform enemyRect = enemy.GetComponent<RectTransform>();
        if (enemyRect == null) return false;

        // UI 공간에서의 거리 계산
        Vector2 characterPos = rectTransform.position;
        Vector2 enemyPos = enemyRect.position;
        float distance = Vector2.Distance(characterPos, enemyPos);

        return distance <= attackRange;
    }

    private float CalculateDamage(bool isCritical)
    {
        float damage = attackDamage;
        if (isCritical)
        {
            damage *= criticalMultiplier;
        }
        return damage;
    }

    private void OnDrawGizmos()
    {
        if (!rectTransform) return;

        Gizmos.color = Color.yellow;
        Vector3 center = rectTransform.position;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}