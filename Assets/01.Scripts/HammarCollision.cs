using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    private SpinnerGameManager gameManager;
    private int hitCount = 1;
    private int maxHits = 6;

    private void Start()
    {
        // 게임 매니저 찾기
        gameManager = FindObjectOfType<SpinnerGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("SpinnerGameManager not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpinnerCircle"))
        {
            Debug.Log("Hit" + hitCount);

            // 게임 매니저를 통해 충돌 처리
            if (gameManager != null)
            {
                gameManager.OnHammarHit();
            }

            hitCount++;
            if (hitCount > maxHits)
            {
                hitCount = 1;
            }
        }
    }
}