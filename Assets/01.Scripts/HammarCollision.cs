using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    [SerializeField] private CoolingBar coolingBar;
    [SerializeField] private float attackPower = 10f;
    
    private CharacterController characterController;
    private int hitCount = 1;
    private int maxHits = 6;
    private bool isFirstHit = true;

    private void Start()
    {
        if (coolingBar == null)
        {
            coolingBar = FindObjectOfType<CoolingBar>();
        }

        characterController = FindObjectOfType<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpinnerCircle"))
        {
            Debug.Log("Hit" + hitCount);

            if (!isFirstHit)  // 첫 번째 히트가 아닐 때만 처리
            {
                // 쿨링 게이지 증가
                if (coolingBar != null)
                {
                    coolingBar.IncrementGauge(attackPower);
                }

                // 공격 실행
                if (characterController != null)
                {
                    characterController.TriggerManualAttack();
                }
            }

            if (isFirstHit)  // 첫 번째 히트 처리
            {
                isFirstHit = false;
            }

            hitCount++;
            if (hitCount > maxHits)
            {
                hitCount = 1;
            }
        }
    }
}