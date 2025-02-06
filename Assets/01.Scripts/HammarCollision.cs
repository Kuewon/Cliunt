using UnityEngine;

public class HammarCollision : MonoBehaviour
{
    [SerializeField] private CoolingBar coolingBar;
    [SerializeField] private float attackPower = 10f;

    private int hitCount = 1;
    private int maxHits = 6;
    private bool isFirstHit = true;  // ù ��° ��Ʈ���� Ȯ���ϴ� ����

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

            if (coolingBar != null && !isFirstHit)  // ù ��° ��Ʈ�� �ƴ� ���� ������ ����
            {
                coolingBar.IncrementGauge(attackPower);
            }

            if (isFirstHit)  // ù ��° ��Ʈ ó�� �� �÷��� ����
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