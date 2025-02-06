using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    private SpinnerGameManager gameManager;
    private CharacterController characterController;
    private int hitCount = 1;
    private int maxHits = 6;
    
    private bool isInitialized = false;
    private float initDelay = 0.5f; // 시작 후 0.5초 동안은 충돌 무시
    private float timer = 0f;

    private void Start()
    {
        gameManager = FindObjectOfType<SpinnerGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("SpinnerGameManager not found in the scene!");
        }

        characterController = FindObjectOfType<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found in the scene!");
        }
    }

    private void Update()
    {
        if (!isInitialized)
        {
            timer += Time.deltaTime;
            if (timer >= initDelay)
            {
                isInitialized = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 초기화가 완료되지 않았다면 충돌 무시
        if (!isInitialized) return;

        if (other.CompareTag("SpinnerCircle"))
        {
            Debug.Log("Hit" + hitCount);

            if (gameManager != null)
            {
                gameManager.OnHammarHit();
            }

            if (characterController != null)
            {
                characterController.TriggerManualAttack();
            }

            hitCount++;
            if (hitCount > maxHits)
            {
                hitCount = 1;
            }
        }
    }
}