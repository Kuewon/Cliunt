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
    
        // 수동 공격을 위해 애니메이터 속도를 1로 복구
        gunAnimator.speed = 1f;
        bodyAnimator.speed = 1f;
        frontHandAnimator.speed = 1f;
        effectAnimator.speed = 1f;
    
        gunAnimator.SetTrigger(MANUALATTACK_TRIGGER);
        bodyAnimator.SetTrigger(MANUALATTACK_TRIGGER);
        frontHandAnimator.SetTrigger(MANUALATTACK_TRIGGER);
        effectAnimator.SetTrigger(MANUALATTACK_TRIGGER);

        float animationLength = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength * 0.5f);
        PerformAttack("수동");

        yield return new WaitForSeconds(animationLength * 0.5f);

        isManualAttackPlaying = false;
    
        // 명시적으로 기본 상태의 길이를 가져옴
        float autoAttackLength = bodyAnimator.runtimeAnimatorController.animationClips
            .First(clip => clip.name == "body_idle").length;
        float speedMultiplier = autoAttackLength / attackInterval;
     
        gunAnimator.speed = speedMultiplier;
        bodyAnimator.speed = speedMultiplier;
        frontHandAnimator.speed = speedMultiplier;
        effectAnimator.speed = speedMultiplier;
    }

    private IEnumerator AutoAttackRoutine()
    {
        float elapsedTime = 0f;
        float lastAttackTime = 0f;

        // 기본 애니메이션 속도 조절
        float originalAnimLength = gunAnimator.GetCurrentAnimatorStateInfo(0).length;
        float speedMultiplier = originalAnimLength / attackInterval;
    
        // 모든 애니메이터의 속도 설정
        gunAnimator.speed = speedMultiplier;
        bodyAnimator.speed = speedMultiplier;
        frontHandAnimator.speed = speedMultiplier;
        effectAnimator.speed = speedMultiplier;

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

                    PerformAttack("자동");
                }
            }
            yield return null;
        }
    }
    
    private void PerformAttack(string attackType)
    {
        var enemies = FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in enemies)
        {
            if (IsEnemyInRange(enemy.gameObject))
            {
                // 크리티컬 판정을 EnemyHealth로 전달
                enemy.TakeDamage(attackDamage, criticalChance, criticalMultiplier);
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

    private void OnDrawGizmos()
    {
        if (!rectTransform) return;

        Gizmos.color = Color.yellow;
        Vector3 center = rectTransform.position;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}