using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float spawnXOffsetPercentage = 0.2f;  // 화면 너비의 20% 위치
    [SerializeField, Range(0f, 1f)] private float spawnYOffsetPercentage = 0.5f;  // 화면 높이의 Y% 위치
    
    [Header("Components")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator frontHandAnimator;
    [SerializeField] private Animator effectAnimator;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioClip autoAttackSound;

    [Header("Base Stats")]
    private float baseAttackDamage = 10f;
    private float baseAttackSpeed = 0.2f;
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
    private bool isTransitioning = false;
    private RectTransform rectTransform;
    private Coroutine autoAttackCoroutine;
    private Coroutine transitionCoroutine;

    private const string MANUALATTACK_TRIGGER = "ManualAttack";
    private const string RUNAUTOATTACK_TRIGGER = "RunAutoAttack";
    private const string RUNMANUALATTACK_TRIGGER = "RunManualAttack";

    private ParallaxBackgroundScroller backgroundScroller;

    private void Awake()
    {
        // 컴포넌트 찾기
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

        backgroundScroller = ParallaxBackgroundScroller.Instance;
        if (backgroundScroller == null)
        {
            backgroundScroller = FindObjectOfType<ParallaxBackgroundScroller>();
            if (backgroundScroller == null)
            {
                Debug.LogError("ParallaxBackgroundScroller를 찾을 수 없습니다!");
            }
        }
        
        if (rectTransform != null)
        {
            // TopIngame 캔버스의 RectTransform 찾기
            var topIngameCanvas = GameObject.FindWithTag("TopIngame")?.GetComponent<RectTransform>();
            if (topIngameCanvas != null)
            {
                float width = topIngameCanvas.rect.width;
                float height = topIngameCanvas.rect.height;
                
                float xPosition = (width * spawnXOffsetPercentage) - (width / 2);
                float yPosition = (height * spawnYOffsetPercentage) - (height / 2);
                
                rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            }
            else
            {
                Debug.LogError("TopIngame 캔버스를 찾을 수 없습니다!");
            }
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

        if (backgroundScroller != null)
        {
            backgroundScroller.OnScrollUpdate += OnScrollStateChanged;
        }

        StartAutoAttack();
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= InitializeStats;

        if (backgroundScroller != null)
        {
            backgroundScroller.OnScrollUpdate -= OnScrollStateChanged;
        }
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

    public void InitializeStats()
    {
        var statsData = GameData.Instance.GetRow("PlayerStats", 0);
        if (statsData == null) return;

        // Get base stats from PlayerStats sheet
        if (TryParseValue(statsData, "baseAttackDamage", out float baseDamage))
            attackDamage = baseDamage;

        // Get equipped revolver's damage
        var userData = UserDataManager.GetCurrentUserData();
        if (userData?.data != null && userData.data.ContainsKey("playerRevolverIndex"))
        {
            int revolverIndex = Convert.ToInt32(userData.data["playerRevolverIndex"]);
            float revolverDamage = GameData.Instance.GetFloat("Revolver", revolverIndex, "revolverBaseDamage", 0f);
            
            // Add revolver damage to base damage
            attackDamage += revolverDamage;
            
            Debug.Log($"✅ 데미지 계산 완료! 기본: {baseDamage}, 리볼버: {revolverDamage}, 최종: {attackDamage}");
        }
        else
        {
            Debug.LogWarning("⚠️ 장착된 리볼버 정보를 찾을 수 없습니다.");
        }

        // Initialize other stats as before
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

    private void OnScrollStateChanged(float progress)
    {
        if (progress > 0 && progress <= 1)
        {
            if (!isTransitioning)
            {
                StartTransition(backgroundScroller.IsScrolling);
            }
        }
    }

    private void StartTransition(bool toRunning)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionRoutine(toRunning));
    }

    private IEnumerator TransitionRoutine(bool toRunning)
    {
        isTransitioning = true;

        if (isManualAttackPlaying)
        {
            yield return new WaitUntil(() => !isManualAttackPlaying);
        }

        StopAutoAttack();
        ResetAllTriggers();
        StartAutoAttack();

        isTransitioning = false;
        transitionCoroutine = null;
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

    private IEnumerator AutoAttackRoutine()
    {
        float elapsedTime = 0f;
        float lastAttackTime = 0f;

        UpdateAutoAttackSpeed();

        while (true)
        {
            if (isInitialized && !isManualAttackPlaying && !isTransitioning)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime - lastAttackTime >= attackInterval)
                {
                    lastAttackTime = elapsedTime;

                    if (autoAttackSound != null)
                    {
                        AudioManager.Instance.PlaySFX(autoAttackSound);
                    }

                    if (backgroundScroller.IsScrolling)
                    {
                        TriggerAllAnimators(RUNAUTOATTACK_TRIGGER);
                    }
                    else
                    {
                        ResetAllTriggers();
                    }

                    PerformAttack(backgroundScroller.IsScrolling ? "달리기 자동" : "자동");
                }
            }
            yield return null;
        }
    }


    public void TriggerManualAttack()
    {
        if (isTransitioning) return;

        PerformAttack(backgroundScroller.IsScrolling ? "달리기 수동" : "수동");

        if (!isManualAttackPlaying)
        {
            StopAutoAttack();
            StartCoroutine(backgroundScroller.IsScrolling ?
                RunManualAttackRoutine() : ManualAttackRoutine());
        }
    }

    private IEnumerator ManualAttackRoutine()
    {
        isManualAttackPlaying = true;
        SetAnimatorSpeeds(1f);
        TriggerAllAnimators(MANUALATTACK_TRIGGER);

        yield return new WaitForSeconds(bodyAnimator.GetCurrentAnimatorStateInfo(0).length);

        isManualAttackPlaying = false;

        if (!isTransitioning)
        {
            StartAutoAttack();
        }
    }

    private IEnumerator RunManualAttackRoutine()
    {
        isManualAttackPlaying = true;
        SetAnimatorSpeeds(1f);
        TriggerAllAnimators(RUNMANUALATTACK_TRIGGER);

        yield return new WaitForSeconds(bodyAnimator.GetCurrentAnimatorStateInfo(0).length);

        isManualAttackPlaying = false;

        if (!isTransitioning)
        {
            StartAutoAttack();
        }
    }

    private void ResetAllTriggers()
    {
        if (!isTransitioning)
        {
            foreach (var animator in new[] { gunAnimator, bodyAnimator, frontHandAnimator, effectAnimator })
            {
                if (animator != null)
                {
                    foreach (var parameter in animator.parameters)
                    {
                        if (parameter.type == AnimatorControllerParameterType.Trigger)
                        {
                            animator.ResetTrigger(parameter.name);
                        }
                    }
                }
            }
        }
    }

    private void TriggerAllAnimators(string triggerName)
    {
        if (!isTransitioning)
        {
            gunAnimator?.SetTrigger(triggerName);
            bodyAnimator?.SetTrigger(triggerName);
            frontHandAnimator?.SetTrigger(triggerName);
            effectAnimator?.SetTrigger(triggerName);
        }
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
        // 적의 BoxCollider2D 가져오기
        BoxCollider2D enemyCollider = enemy.GetComponent<BoxCollider2D>();
        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        
        if (enemyCollider == null || playerCollider == null) return false;

        // 콜라이더의 중심점 위치 계산
        Vector2 enemyCenter = enemyCollider.bounds.center;
        Vector2 playerCenter = playerCollider.bounds.center;
        
        // 두 콜라이더 중심점 사이의 거리 계산
        float distance = Vector2.Distance(playerCenter, enemyCenter);

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