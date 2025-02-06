using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    [SerializeField] private CoolingBar coolingBar;
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private int maxHits = 6;
    private CharacterController characterController;
    private int hitCount = 1;
    private bool isFirstHit = true;
    private bool wasLocked = false;

    private bool ignoreInitialCollisions = true; // ✅ 초기 충돌 무시 변수

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

        // 🔴 스프레드시트 제거 후 하드코딩 적용
        attackPower = 10f;
        maxHits = 6;
    }

    private void Update()
    {
        // ✅ 첫 번째 프레임이 지나면 충돌 감지 활성화
        if (ignoreInitialCollisions)
        {
            ignoreInitialCollisions = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ 초기 충돌 무시
        if (ignoreInitialCollisions) return;

        if (other.CompareTag("SpinnerCircle"))
        {
            if (!wasLocked)
            {
                if (hitCount <= maxHits)
                {
                    Debug.Log($"Hit {hitCount}");
                }

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
