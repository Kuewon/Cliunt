using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator frontHandAnimator;
    [SerializeField] private Animator effectAnimator;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioClip autoAttackSound;

    [Header("Base Stats")]
    private float baseAttackDamage = 10f;
    private float baseAttackSpeed = 0.2f; // 초당 공격 횟수
    private float baseAttackRange = 3f;
    private float baseCriticalChance = 0.1f;
    private float baseCriticalMultiplier = 1.5f;

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
        if (gunAnimator == null)
        {
            Transform gunTransform = transform.Find("Gun");
            if (gunTransform != null)
            {
                gunAnimator = gunTransform.GetComponent<Animator>();
            }
        }
        
        if (bodyAnimator == null)
        {
            Transform bodyTransform = transform.Find("Body");
            if (bodyTransform != null)
            {
                bodyAnimator = bodyTransform.GetComponent<Animator>();
            }
        }
        
        if (frontHandAnimator == null)
        {
            Transform frontHandTransform = transform.Find("FrontHand");
            if (frontHandTransform != null)
            {
                frontHandAnimator = frontHandTransform.GetComponent<Animator>();
            }
        }
        
        if (effectAnimator == null)
        {
            Transform effectTransform = transform.Find("GunEffect");
            if (effectTransform != null)
            {
                effectAnimator = effectTransform.GetComponent<Animator>();
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

    private bool CanAttack()
    {
        // 배경이 스크롤 중일 때는 공격 불가
        return !BackgroundScroller.Instance.IsScrolling;
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
        // 기존 코드 유지
        result = 0f;
        if (!data.ContainsKey(key)) return false;

        string value = data[key]?.ToString();
        if (string.IsNullOrEmpty(value)) return false;

        return float.TryParse(value, out result);
    }

    public void TriggerManualAttack()
    {
        // 스크롤 중이면 수동 공격 불가
        if (!CanAttack()) return;
        
        StartCoroutine(ManualAttackRoutine());
    }

    private IEnumerator ManualAttackRoutine()
    {
        isManualAttackPlaying = true;

        bodyAnimator.ResetTrigger(AUTOATTACK_TRIGGER);
        bodyAnimator.SetTrigger(MANUALATTACK_TRIGGER);
        
        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger(AUTOATTACK_TRIGGER);
            gunAnimator.SetTrigger(MANUALATTACK_TRIGGER);
        }

        float animationLength = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;

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
            if (isInitialized && !isManualAttackPlaying && CanAttack())
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime - lastAttackTime >= attackInterval)
                {
                    lastAttackTime = elapsedTime;

                    // 자동 공격 사운드 재생
                    if (autoAttackSound != null)
                    {
                        AudioManager.Instance.PlaySFX(autoAttackSound);
                    }

                    // 공격 애니메이션 길이만큼만 정확히 대기
                    float attackAnimLength = gunAnimator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(attackAnimLength);

                    PerformAttack("자동");

                    // 다음 공격까지 남은 시간 계산
                    float remainingTime = attackInterval - attackAnimLength;
                    if (remainingTime > 0)
                    {
                        yield return new WaitForSeconds(remainingTime);
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