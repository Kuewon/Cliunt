using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    [SerializeField] private CoolingBar coolingBar;
    [SerializeField] private float attackPower = 10f;

    private int hitCount = 1;
    private int maxHits = 6;
    private bool isFirstHit = true;  // 첫 번째 히트인지 확인하는 변수

    private void Start()
    {
        if (coolingBar == null)
        {
            coolingBar = FindObjectOfType<CoolingBar>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpinnerCircle"))
        {
            Debug.Log("Hit" + hitCount);

            if (coolingBar != null && !isFirstHit)  // 첫 번째 히트가 아닐 때만 게이지 증가
            {
                coolingBar.IncrementGauge(attackPower);
            }

            if (isFirstHit)  // 첫 번째 히트 처리 후 플래그 변경
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