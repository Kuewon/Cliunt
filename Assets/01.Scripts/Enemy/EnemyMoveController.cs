using UnityEngine;
using UnityEngine.UI;

public class EnemyMoveController : MonoBehaviour
{
    [Header("Enemy Type")]
    private bool isRangedEnemy = false;  // 원거리 적 여부
    public void SetRangedEnemy(bool isRanged) => isRangedEnemy = isRanged;
    
    private float moveSpeed = 150f;
    private float attackRange = 1f;
    private float attackInterval = 1f;
    private float attackDamage = 10f;
    private float enemyDropGold = 10.0f;
    public float _enemyDropGold => enemyDropGold;

    private bool canMove = false;
    private Animator animator;
    private RectTransform playerRectTransform;
    private RectTransform myRectTransform;
    private Image image;
    private float attackTimer;
    private bool canAttack = true;
    private Vector2 initialPosition;

    private EnemyHealth enemyHealth;
    private bool isDestroyed = false;

    // 애니메이션 파라미터 상수
    private readonly string PARAM_ATTACK = "Attack";  // 트리거
    private readonly string PARAM_IS_WALKING = "IsWalking"; // bool
    
    private RangedHitEffectsManager hitEffectsManager;
    private BoxCollider2D myCollider;
    private BoxCollider2D playerCollider;
    
    public Vector2 InitialPosition { get; set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        myRectTransform = GetComponent<RectTransform>();
        enemyHealth = GetComponent<EnemyHealth>();
        myCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDeath += HandleDeath;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRectTransform = player.GetComponent<RectTransform>();
            playerCollider = player.GetComponent<BoxCollider2D>();
        }

        initialPosition = myRectTransform.anchoredPosition;

        // 시작할 때는 무조건 걷는 상태
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_WALKING, true);
        }

        if (ParallaxBackgroundScroller.Instance != null)
        {
            if (ParallaxBackgroundScroller.Instance.IsScrolling)
            {
                ParallaxBackgroundScroller.Instance.OnScrollUpdate += SyncWithBackground;
                ParallaxBackgroundScroller.Instance.OnScrollComplete += StartMoving;
            }
            else
            {
                StartMoving();
            }
        }
        else
        {
            StartMoving();
        }

        if (WaveMovementController.Instance != null)
        {
            WaveMovementController.Instance.RegisterEnemy(this);
        }
        
        if (isRangedEnemy)
        {
            hitEffectsManager = FindObjectOfType<RangedHitEffectsManager>();
            if (hitEffectsManager == null)
            {
                Debug.LogWarning("RangedHitEffectsManager를 찾을 수 없습니다!");
            }
        }
    }


    private void HandleDeath()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        SetMovementEnabled(false);

        if (ParallaxBackgroundScroller.Instance != null)
        {
            ParallaxBackgroundScroller.Instance.OnScrollUpdate -= SyncWithBackground;
            ParallaxBackgroundScroller.Instance.OnScrollComplete -= StartMoving;
        }

        if (WaveMovementController.Instance != null)
        {
            WaveMovementController.Instance.UnregisterEnemy(this);
        }

        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDeath -= HandleDeath;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (image != null)
        {
            image.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (WaveMovementController.Instance != null)
        {
            WaveMovementController.Instance.UnregisterEnemy(this);
        }
    }

    public void SetStats(float damage, float speed, float movement, float range, float gold)
    {
        if (isDestroyed) return;

        attackDamage = damage;
        attackInterval = 1f / speed;
        moveSpeed = movement * 100f;
        attackRange = range;
        enemyDropGold = gold;
    }

    private void SyncWithBackground(float scrollProgress)
    {
        if (isDestroyed) return;

        // ParallaxBackgroundScroller의 스크롤 거리 사용
        Vector2 newPos = initialPosition;
        // 화면 너비만큼 스크롤
        float totalScroll = GetComponent<RectTransform>().rect.width;
        newPos.x -= totalScroll * scrollProgress;
        myRectTransform.anchoredPosition = newPos;
    }

    public void StartMoving()
    {
        if (isDestroyed) return;

        if (ParallaxBackgroundScroller.Instance != null)
        {
            ParallaxBackgroundScroller.Instance.OnScrollUpdate -= SyncWithBackground;
            ParallaxBackgroundScroller.Instance.OnScrollComplete -= StartMoving;
        }
        canMove = true;
    }

    private void Update()
    {
        if (isDestroyed || this == null || playerRectTransform == null || myRectTransform == null)
            return;

        if (!canMove) return;

        bool inRange = IsInAttackRange();

        if (inRange)
        {
            // 공격 범위 안에 있으면 걷기 중지
            if (animator != null)
            {
                animator.SetBool(PARAM_IS_WALKING, false);
            }

            if (canAttack)
            {
                Attack();
            }
        }
        else
        {
            // 공격 범위 밖이면 계속 걷기
            if (animator != null)
            {
                animator.SetBool(PARAM_IS_WALKING, true);
            }

            // 왼쪽으로 이동
            Vector2 currentPos = myRectTransform.anchoredPosition;
            float moveDistance = moveSpeed * Time.deltaTime;
            Vector2 movement = Vector2.left * moveDistance;
            Vector2 newPosition = currentPos + movement;
            myRectTransform.anchoredPosition = newPosition;
        }

        // 공격 쿨타임 관리
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                canAttack = true;
                attackTimer = 0f;
            }
        }
    }

    private void Attack()
    {
        if (isDestroyed) return;

        // 공격 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger(PARAM_ATTACK);
        }

        // 플레이어에게 데미지
        PlayerHealth playerHealth = playerRectTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);

            // 원거리 적일 경우 타격 이펙트 생성
            if (isRangedEnemy && hitEffectsManager != null)
            {
                hitEffectsManager.SpawnHitEffect();
            }
        }

        canAttack = false;
        attackTimer = 0f;
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (isDestroyed) return;

        canMove = enabled;

        // 이동 불가능하거나 공격 범위 안이면 걷기 중지
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_WALKING, enabled && !IsInAttackRange());
        }
    }

    private bool IsInAttackRange()
    {
        if (isDestroyed || playerCollider == null || myCollider == null) return false;

        // 콜라이더의 중심점 위치 계산
        Vector2 enemyCenter = myCollider.bounds.center;
        Vector2 playerCenter = playerCollider.bounds.center;
        
        // 두 콜라이더 중심점 사이의 거리 계산
        float distance = Vector2.Distance(enemyCenter, playerCenter);

        return distance <= attackRange;
    }
}