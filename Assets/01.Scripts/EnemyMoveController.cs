using UnityEngine;

public class EnemyMoveController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackDamage = 10f;

    private Animator animator;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private float attackTimer;
    private bool canAttack = true;

    private void Awake()
    {
        // Awake에서 컴포넌트들을 가져옵니다
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 컴포넌트가 없다면 경고 메시지를 출력합니다
        if (animator == null)
            Debug.LogWarning("Animator component is missing on the enemy!");
        if (spriteRenderer == null)
            Debug.LogWarning("SpriteRenderer component is missing on the enemy!");
    }

    private void Start()
    {
        // Start에서는 플레이어만 찾습니다
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found in the scene!");
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 플레이어 방향으로 이동
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 스프라이트 방향 설정
        spriteRenderer.flipX = direction.x < 0;

        if (distanceToPlayer > attackRange)
        {
            // 공격 범위 밖이면 이동
            transform.Translate(direction * moveSpeed * Time.deltaTime);
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            // 공격 범위 안이면 공격
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
            }
            if (canAttack)
            {
                Attack();
            }
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
    }

    private void Attack()
    {
        // 애니메이션 트리거 설정
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 플레이어에게 데미지 전달
        CharacterHealth playerHealth = playerTransform.GetComponent<CharacterHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        canAttack = false;
    }

    // 공격 범위 시각화 (디버그용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}