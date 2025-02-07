using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    public CoolingBar coolingBar;
    public float attackPower; // 구글
    public int maxHits = 6; // 지금은 총알의 역할을 대신하고있음. 나중에 bullet으로 바꿔야함.
    private CharacterController characterController;
    private int hitCount = 1;
    private bool isFirstHit = true;
    private bool wasLocked = false;
    private bool ignoreInitialCollisions = true; // 초기 충돌 무시 변수

    private void Awake()
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

    private void Start()
    {
        attackPower = (float)GameData.Instance.GetRow("PlayerUpgrade", 0)["attackPower"];
        
    }

    private void Update()
    {
        if (ignoreInitialCollisions)
        {
            ignoreInitialCollisions = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
