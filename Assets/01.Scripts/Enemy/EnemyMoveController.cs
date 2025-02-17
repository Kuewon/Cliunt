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
        if (isDestroyed) return; // 중복 처리 방지
        
        isDestroyed = true;
        SetMovementEnabled(false);
        
        // 모든 이벤트 구독 해제
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

        // 애니메이터 비활성화
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 이미지 컴포넌트 비활성화
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

        if (animator != null && !isDestroyed)
        {
            animator.SetBool("IsWalking", true);
        }
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
        
        if (animator != null && !isDestroyed)
        {
            animator.SetBool("IsWalking", true);
        }
    }

    private void Update()
    {
        if (isDestroyed || this == null || playerRectTransform == null || myRectTransform == null) 
            return;

        if (!canMove) return;

        bool inRange = IsInAttackRange();

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

            Vector2 currentPos = myRectTransform.anchoredPosition;
            float moveDistance = moveSpeed * Time.deltaTime;
            Vector2 movement = Vector2.left * moveDistance;
            Vector2 newPosition = currentPos + movement;
            myRectTransform.anchoredPosition = newPosition;
        }

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
        if (isDestroyed) return;
        
        canMove = enabled;
        if (animator != null)
        {
            animator.SetBool("IsWalking", enabled && !IsInAttackRange());
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