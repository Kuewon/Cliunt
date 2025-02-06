using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask enemyLayer;

    private float attackInterval = 5f;
    private float autoAttackRange = 1f;
    private float manualAttackRange = 2f;
    
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

    // 즉시 데미지를 주는 메서드
    public void TriggerManualAttack()
    {
        // 애니메이션만 코루틴으로 처리
        StartCoroutine(PlayManualAttackAnimation());
        // 데미지는 즉시 적용
        CheckHitInRange(manualAttackRange, manualAttackDamage);
    }

    private void CheckHitInRange(float range, float damage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                HitEffect hitEffect = enemy.GetComponent<HitEffect>();
                if (hitEffect != null)
                {
                    hitEffect.PlayHitEffect();
                }
                Debug.Log($"플레이어가 적에게 {damage} 데미지를 입혔습니다!");
            }
        }
    }

    // 애니메이션만 처리하는 코루틴
    private IEnumerator PlayManualAttackAnimation()
    {
        if (isManualAttackPlaying) yield break;

        isManualAttackPlaying = true;
        animator.ResetTrigger(AUTOATTACK_TRIGGER);
        animator.SetTrigger(MANUALATTACK_TRIGGER);

        // 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        
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
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
                CheckHitInRange(autoAttackRange, autoAttackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, autoAttackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, manualAttackRange);
    }
}