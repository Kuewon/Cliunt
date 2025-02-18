using UnityEngine;
using UnityEngine.UI;

public class EnemyMoveController : MonoBehaviour
{
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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        myRectTransform = GetComponent<RectTransform>();
        enemyHealth = GetComponent<EnemyHealth>();
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
        }

        initialPosition = myRectTransform.anchoredPosition;

        // 시작할 때는 무조건 걷는 상태
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_WALKING, true);
        }

        if (BackgroundScroller.Instance != null)
        {
            if (BackgroundScroller.Instance.IsScrolling)
            {
                BackgroundScroller.Instance.OnScrollUpdate += SyncWithBackground;
                BackgroundScroller.Instance.OnScrollComplete += StartMoving;
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
    }

    private void HandleDeath()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        SetMovementEnabled(false);

        if (BackgroundScroller.Instance != null)
        {
            BackgroundScroller.Instance.OnScrollUpdate -= SyncWithBackground;
            BackgroundScroller.Instance.OnScrollComplete -= StartMoving;
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
        HandleDeath();
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

        float totalScroll = BackgroundScroller.Instance.GetScrollAmount();
        Vector2 newPos = initialPosition;
        newPos.x -= totalScroll * scrollProgress;
        myRectTransform.anchoredPosition = newPos;
    }

    public void StartMoving()
    {
        if (isDestroyed) return;

        if (BackgroundScroller.Instance != null)
        {
            BackgroundScroller.Instance.OnScrollUpdate -= SyncWithBackground;
            BackgroundScroller.Instance.OnScrollComplete -= StartMoving;
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
        if (isDestroyed || playerRectTransform == null) return false;

        Vector2 enemyPos = myRectTransform.position;
        Vector2 playerPos = playerRectTransform.position;
        float distance = Vector2.Distance(enemyPos, playerPos);

        return distance <= attackRange;
    }
}