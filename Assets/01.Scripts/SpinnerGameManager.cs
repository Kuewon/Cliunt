using UnityEngine;

public class SpinnerGameManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private SpinnerController spinnerController;
    [SerializeField] private HammarCollision hammarCollision;
    [SerializeField] private CoolingBar coolingBar;

    [Header("Game Settings")]
    [SerializeField] private float attackPower = 10f;    // ���ݷ� (������ ������)

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;  // ����� �α� ǥ�� ����

    private void Start()
    {
        // ������Ʈ �ڵ� ã�� (�Ҵ���� ���� ���)
        if (spinnerController == null)
            spinnerController = FindObjectOfType<SpinnerController>();

        if (hammarCollision == null)
            hammarCollision = FindObjectOfType<HammarCollision>();

        if (coolingBar == null)
            coolingBar = FindObjectOfType<CoolingBar>();

        // ������Ʈ ��ȿ�� �˻�
        if (spinnerController == null || hammarCollision == null || coolingBar == null)
        {
            Debug.LogError("Required components are missing in SpinnerGameManager!");
            enabled = false;
            return;
        }

        if (showDebugLogs)
            Debug.Log("SpinnerGameManager initialized successfully.");
    }

    // Hammar �浹 �� ȣ��Ǵ� �޼���
    public void OnHammarHit()
    {
        if (coolingBar != null)
        {
            bool success = coolingBar.IncrementGauge(attackPower);

            if (showDebugLogs)
            {
                if (success)
                    Debug.Log($"Gauge increased by {attackPower}. Current: {coolingBar.GetGaugePercentage():F1}%");
                else
                    Debug.Log("Gauge increase failed - Spinner is locked!");
            }
        }
    }

    // ���ǳ� ��� ���� ���� �� ȣ��Ǵ� �޼���
    public void OnSpinnerLocked(bool locked)
    {
        if (spinnerController != null)
        {
            // ���⿡ ���ǳ� ���/���� ���� ���� �߰�
            if (showDebugLogs)
                Debug.Log($"Spinner is now {(locked ? "locked" : "unlocked")}");
        }
    }

    // attackPower ���� �������� �޼��� (�ʿ��� ���)
    public float GetAttackPower()
    {
        return attackPower;
    }

    // ���� ���� ���� �޼��� (�ʿ��� ���)
    public void ResetGame()
    {
        if (showDebugLogs)
            Debug.Log("Resetting game state...");

        // ���⿡ ���� ���� ���� �߰�
    }

#if UNITY_EDITOR
    // �����Ϳ��� �׽�Ʈ�� ���� �޼����
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }
#endif
}