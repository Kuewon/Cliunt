using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private Coroutine autoAttackCoroutine;

    private const string MANUALATTACK_TRIGGER = "ManualAttack";
    private const string RUNAUTOATTACK_TRIGGER = "RunAutoAttack";
    private const string RUNMANUALATTACK_TRIGGER = "RunManualAttack";

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

        StartAutoAttack();
    }

    private void StartAutoAttack()
    {
        if (autoAttackCoroutine != null)
        {
            StopCoroutine(autoAttackCoroutine);
        }
        autoAttackCoroutine = StartCoroutine(AutoAttackRoutine());
    }

    private void StopAutoAttack()
    {
        if (autoAttackCoroutine != null)
        {
            StopCoroutine(autoAttackCoroutine);
            autoAttackCoroutine = null;
        }
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
        // 기존 코드 유지
        result = 0f;
        if (!data.ContainsKey(key)) return false;

        string value = data[key]?.ToString();
        if (string.IsNullOrEmpty(value)) return false;

        return float.TryParse(value, out result);
    }

    public void TriggerManualAttack()
    {
        // 데미지는 즉시 적용
        if (BackgroundScroller.Instance.IsScrolling)
        {
            PerformAttack("달리기 수동");
        }
        else
        {
            PerformAttack("수동");
        }

        // 애니메이션이 재생 중이 아닐 때만 새로운 애니메이션 시작
        if (!isManualAttackPlaying)
        {
            // 현재 자동 공격 중지
            StopAutoAttack();

            // 달리기 여부에 따라 다른 수동 공격 애니메이션 실행
            if (BackgroundScroller.Instance.IsScrolling)
            {
                StartCoroutine(RunManualAttackRoutine());
            }
            else
            {
                StartCoroutine(ManualAttackRoutine());
            }
        }
    }

    private IEnumerator ManualAttackRoutine()
    {
        isManualAttackPlaying = true;

        // 수동 공격을 위해 애니메이터 속도를 1로 복구
        SetAnimatorSpeeds(1f);

        // 일반 수동 공격 트리거
        TriggerAllAnimators(MANUALATTACK_TRIGGER);

        float animationLength = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;

        // 애니메이션만 재생하고 데미지는 적용하지 않음
        yield return new WaitForSeconds(animationLength);

        isManualAttackPlaying = false;
        StartAutoAttack();
    }

    private IEnumerator RunManualAttackRoutine()
    {
        isManualAttackPlaying = true;

        SetAnimatorSpeeds(1f);

        // 달리기 수동 공격 트리거
        TriggerAllAnimators(RUNMANUALATTACK_TRIGGER);

        float animationLength = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;

        // 애니메이션만 재생하고 데미지는 적용하지 않음
        yield return new WaitForSeconds(animationLength);

        isManualAttackPlaying = false;
        StartAutoAttack();
    }

    private IEnumerator AutoAttackRoutine()
    {
        float elapsedTime = 0f;
        float lastAttackTime = 0f;

        UpdateAutoAttackSpeed();

        while (true)
        {
            if (isInitialized && !isManualAttackPlaying)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime - lastAttackTime >= attackInterval)
                {
                    lastAttackTime = elapsedTime;

                    if (autoAttackSound != null)
                    {
                        AudioManager.Instance.PlaySFX(autoAttackSound);
                    }

                    // 달리기 중일 때만 RunAutoAttack 트리거 사용
                    if (BackgroundScroller.Instance.IsScrolling)
                    {
                        TriggerAllAnimators(RUNAUTOATTACK_TRIGGER);
                    }

                    PerformAttack(BackgroundScroller.Instance.IsScrolling ? "달리기 자동" : "자동");
                }
            }
            yield return null;
        }
    }

    private void TriggerAllAnimators(string triggerName)
    {
        gunAnimator?.SetTrigger(triggerName);
        bodyAnimator?.SetTrigger(triggerName);
        frontHandAnimator?.SetTrigger(triggerName);
        effectAnimator?.SetTrigger(triggerName);
    }

    private void SetAnimatorSpeeds(float speed)
    {
        gunAnimator.speed = speed;
        bodyAnimator.speed = speed;
        frontHandAnimator.speed = speed;
        effectAnimator.speed = speed;
    }

    private void UpdateAutoAttackSpeed()
    {
        float autoAttackLength = bodyAnimator.runtimeAnimatorController.animationClips
            .First(clip => clip.name == "body_idle").length;
        float speedMultiplier = autoAttackLength / attackInterval;
        SetAnimatorSpeeds(speedMultiplier);
    }

    private void PerformAttack(string attackType)
    {
        var enemies = FindObjectsOfType<EnemyHealth>();

        EnemyHealth nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy != null && IsEnemyInRange(enemy.gameObject))
            {
                float distance = Vector2.Distance(rectTransform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        if (nearestEnemy != null)
        {
            nearestEnemy.TakeDamage(attackDamage, criticalChance, criticalMultiplier);
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

    private void OnDrawGizmos()
    {
        if (!rectTransform) return;

        Gizmos.color = Color.yellow;
        Vector3 center = rectTransform.position;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}