using UnityEngine;
using UnityEngine.UI;

public class EnemyMoveController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackDamage = 10f;

    [SerializeField] private float enemyDropGold = 10.0f;
    public float _enemyDropGold => enemyDropGold;

    private Animator animator;
    private Transform playerTransform;
    private Image image;
    private float attackTimer;
    private bool canAttack = true;
    private RectTransform rectTransform;

    // 새로운 변수들
    private Vector2 constantMovementDirection = Vector2.left;
    private float originalXPosition;
    private bool isWithinAttackArea = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (animator == null)
            Debug.LogWarning("Animator component is missing on the enemy!");
        if (image == null)
            Debug.LogWarning("Image component is missing on the enemy!");
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found in the scene!");
        }

        originalXPosition = rectTransform.anchoredPosition.x;
    }

    public void SetStats(float damage, float speed, float movement, float range, float gold)
    {
        attackDamage = damage;
        attackInterval = 1f / speed;
        moveSpeed = movement;
        attackRange = range;
        enemyDropGold = gold;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 항상 왼쪽으로 이동
        Vector2 currentPosition = rectTransform.anchoredPosition;
        Vector2 newPosition = currentPosition + (constantMovementDirection * moveSpeed * Time.deltaTime);
        rectTransform.anchoredPosition = newPosition;

        // 플레이어와의 거리 계산 (X축만 고려)
        float distanceToPlayer = Mathf.Abs(rectTransform.anchoredPosition.x - playerTransform.position.x);

        // 적 방향 설정 (항상 왼쪽을 바라봄)
        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // 걷는 애니메이션 처리
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        // 공격 범위 체크 및 공격 처리
        if (distanceToPlayer <= attackRange && !isWithinAttackArea)
        {
            isWithinAttackArea = true;
            if (canAttack)
            {
                Attack();
            }
        }
        else if (distanceToPlayer > attackRange)
        {
            isWithinAttackArea = false;
        }

        // 공격 쿨다운 관리
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                canAttack = true;
                attackTimer = 0f;
            }
        }

        // 공격 범위 안에 있을 때 계속 공격
        if (isWithinAttackArea && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        CharacterHealth playerHealth = playerTransform.GetComponent<CharacterHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        canAttack = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}