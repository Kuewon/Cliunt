using UnityEngine;

public class EnemyMoveController  : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;  // 이동 속도
    [SerializeField] private Animator animator;     // 적 애니메이터
    private SpriteRenderer spriteRenderer;          // 스프라이트 방향 전환용

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 왼쪽으로 이동하므로 스프라이트 좌우 반전
        spriteRenderer.flipX = true;
    }

    private void Update()
    {
        // 왼쪽으로 이동 (-x 방향)
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        
        // 걷기 애니메이션 파라미터 설정 (필요한 경우)
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
    }
}