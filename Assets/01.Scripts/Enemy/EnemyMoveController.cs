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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        myRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRectTransform = player.GetComponent<RectTransform>();
        }

        initialPosition = myRectTransform.anchoredPosition;

        // 배경 스크롤 중인지 확인
        if (BackgroundScroller.Instance != null)
        {
            if (BackgroundScroller.Instance.IsScrolling)
            {
                // 스크롤 중이면 이벤트 구독
                BackgroundScroller.Instance.OnScrollUpdate += SyncWithBackground;
                BackgroundScroller.Instance.OnScrollComplete += StartMoving;
            }
            else
            {
                // 스크롤 중이 아니면 바로 이동 시작
                StartMoving();
            }
        }
        else
        {
            // BackgroundScroller가 없으면 바로 이동 시작
            StartMoving();
        }

        // WaveMovementController에 등록
        if (WaveMovementController.Instance != null)
        {
            WaveMovementController.Instance.RegisterEnemy(this);
        }
    }

    private void OnDestroy()
    {
        if (BackgroundScroller.Instance != null)
        {
            BackgroundScroller.Instance.OnScrollUpdate -= SyncWithBackground;
            BackgroundScroller.Instance.OnScrollComplete -= StartMoving;
        }

        // WaveMovementController에서 제거
        if (WaveMovementController.Instance != null)
        {
            WaveMovementController.Instance.UnregisterEnemy(this);
        }
    }

    public void SetStats(float damage, float speed, float movement, float range, float gold)
    {
        attackDamage = damage;
        attackInterval = 1f / speed;
        moveSpeed = movement * 100f;
        attackRange = range;
        enemyDropGold = gold;
    }

    // 배경 스크롤과 함께 이동
    private void SyncWithBackground(float scrollProgress)
    {
        float totalScroll = BackgroundScroller.Instance.GetScrollAmount();
        Vector2 newPos = initialPosition;
        newPos.x -= totalScroll * scrollProgress;
        myRectTransform.anchoredPosition = newPos;

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
    }

    // 스크롤이 끝나면 자체 이동 시작
    public void StartMoving()
    {
        if (BackgroundScroller.Instance != null)
        {
            BackgroundScroller.Instance.OnScrollUpdate -= SyncWithBackground;
            BackgroundScroller.Instance.OnScrollComplete -= StartMoving;
        }
        canMove = true;
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
    }

    private void Update()
    {
        if (playerRectTransform == null || myRectTransform == null) return;
        if (!canMove) return;

        // 플레이어와의 거리 계산
        bool inRange = IsInAttackRange();

        // 공격 범위 체크
        if (inRange)
        {
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
            }

            if (canAttack)
            {
                Attack();
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }

            // 이동 처리
            Vector2 currentPos = myRectTransform.anchoredPosition;
            float moveDistance = moveSpeed * Time.deltaTime;
            Vector2 movement = Vector2.left * moveDistance;
            Vector2 newPosition = currentPos + movement;
            myRectTransform.anchoredPosition = newPosition;
        }

        // 공격 쿨다운
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
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

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
        canMove = enabled;
        if (animator != null)
        {
            animator.SetBool("IsWalking", enabled && !IsInAttackRange());
        }
    }
    
    private bool IsInAttackRange()
    {
        if (playerRectTransform == null) return false;
    
        Vector2 enemyPos = myRectTransform.position;
        Vector2 playerPos = playerRectTransform.position;
        float distance = Vector2.Distance(enemyPos, playerPos);
    
        return distance <= attackRange;
    }
}