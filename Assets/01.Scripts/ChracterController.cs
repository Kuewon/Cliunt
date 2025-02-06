using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask enemyLayer;  // 적 레이어 설정용
    
    private float attackInterval = 5f;
    private float autoAttackRange = 1f;  // 자동 공격 사거리
    private float manualAttackRange = 2f;  // 수동 공격 사거리
    
    // 데미지 설정
    private float autoAttackDamage = 10f;
    private float manualAttackDamage = 15f;

    private readonly string AUTOATTACK_TRIGGER = "AutoAttack";
    private readonly string MANUALATTACK_TRIGGER = "ManualAttack";

    private bool isManualAttackPlaying = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(PlayManualAttack());
        }
    }

    private void CheckHitInRange(float range, float damage)
    {
        // 원형 범위 내의 모든 적 감지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // 적 체력 시스템에 데미지 전달
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            // 히트 이펙트 생성 (선택사항)
            CreateHitEffect(enemy.transform.position);
        }
    }

    private void CreateHitEffect(Vector2 hitPosition)
    {
        // 여기에 히트 이펙트 파티클 시스템 생성 코드 추가
        // 예: Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
    }

    private IEnumerator PlayManualAttack()
    {
        isManualAttackPlaying = true;
        
        animator.ResetTrigger(AUTOATTACK_TRIGGER);
        animator.SetTrigger(MANUALATTACK_TRIGGER);
        
        // 애니메이션 중간 지점에서 데미지 판정
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
        CheckHitInRange(manualAttackRange, manualAttackDamage);
        
        // 나머지 애니메이션 진행
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
        
        isManualAttackPlaying = false;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            
            if (!isManualAttackPlaying)
            {
                animator.SetTrigger(AUTOATTACK_TRIGGER);
                // 자동 공격의 데미지 판정
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
                CheckHitInRange(autoAttackRange, autoAttackDamage);
            }
        }
    }

    // 범위 시각화 (디버그용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, autoAttackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, manualAttackRange);
    }
}