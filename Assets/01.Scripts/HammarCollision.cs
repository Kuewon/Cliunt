using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    [SerializeField] private CoolingBar coolingBar;
    [SerializeField] private float attackPower = 10f;

    private CharacterController characterController;
    private int hitCount = 1;
    private int maxHits = 6;
    private bool isFirstHit = true;
    private bool wasLocked = false;  // max 상태였는지 기록

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

    private void Update()
    {
        // CoolingBar 상태 업데이트
        if (coolingBar != null)
        {
            if (coolingBar.IsLocked)
            {
                wasLocked = true;
            }
            else if (wasLocked && !coolingBar.IsLocked)
            {
                // 잠금이 해제되면(게이지가 0이 되면) 리셋
                wasLocked = false;
                isFirstHit = true;
                hitCount = 1;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpinnerCircle"))
        {
            // wasLocked가 true면 게이지가 0이 될 때까지 hit 처리 안함
            if (!wasLocked)
            {
                Debug.Log("Hit" + hitCount);

                if (!isFirstHit)
                {
                    if (coolingBar != null)
                    {
                        coolingBar.IncrementGauge(attackPower);
                    }
                    if (characterController != null)
                    {
                        characterController.TriggerManualAttack();
                    }
                }

                if (isFirstHit)
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
}