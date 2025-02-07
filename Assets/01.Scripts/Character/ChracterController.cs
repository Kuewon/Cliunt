using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
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

    private const string AUTOATTACK_TRIGGER = "AutoAttack";
    private const string MANUALATTACK_TRIGGER = "ManualAttack";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
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

        // 애니메이션 재생
        animator.ResetTrigger(AUTOATTACK_TRIGGER);
        animator.SetTrigger(MANUALATTACK_TRIGGER);

        // 애니메이션 정보 가져오기
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        // 애니메이션 중간 지점까지 대기 후 데미지 적용
        yield return new WaitForSeconds(animationLength * 0.5f);
        PerformAttack("수동");

        // 나머지 애니메이션 시간 대기
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

                // 공격 간격이 지났는지 확인
                if (elapsedTime - lastAttackTime >= attackInterval)
                {
                    lastAttackTime = elapsedTime;

                    // 애니메이션 재생
                    animator.SetTrigger(AUTOATTACK_TRIGGER);

                    // 애니메이션의 중간 지점에서 데미지 적용
                    float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animationLength * 0.5f);

                    PerformAttack("자동");

                    // 남은 애니메이션 시간 대기
                    yield return new WaitForSeconds(animationLength * 0.5f);

                    // 남은 공격 간격 대기
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

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent(out EnemyHealth enemyHealth))
            {
                // isCritical 파라미터 전달 확인
                enemyHealth.TakeDamage(finalDamage, isCritical);
            }
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}