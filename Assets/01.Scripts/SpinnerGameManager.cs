using UnityEngine;

public class SpinnerGameManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private SpinnerController spinnerController;
    [SerializeField] private HammarCollision hammarCollision;
    [SerializeField] private CoolingBar coolingBar;

    [Header("Game Settings")]
    [SerializeField] private float attackPower = 10f;    // 공격력 (게이지 증가량)

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;  // 디버그 로그 표시 여부

    private void Start()
    {
        // 컴포넌트 자동 찾기 (할당되지 않은 경우)
        if (spinnerController == null)
            spinnerController = FindObjectOfType<SpinnerController>();

        if (hammarCollision == null)
            hammarCollision = FindObjectOfType<HammarCollision>();

        if (coolingBar == null)
            coolingBar = FindObjectOfType<CoolingBar>();

        // 컴포넌트 유효성 검사
        if (spinnerController == null || hammarCollision == null || coolingBar == null)
        {
            Debug.LogError("Required components are missing in SpinnerGameManager!");
            enabled = false;
            return;
        }

        if (showDebugLogs)
            Debug.Log("SpinnerGameManager initialized successfully.");
    }

    // Hammar 충돌 시 호출되는 메서드
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

    // 스피너 잠금 상태 변경 시 호출되는 메서드
    public void OnSpinnerLocked(bool locked)
    {
        if (spinnerController != null)
        {
            // 여기에 스피너 잠금/해제 관련 로직 추가
            if (showDebugLogs)
                Debug.Log($"Spinner is now {(locked ? "locked" : "unlocked")}");
        }
    }

    // attackPower 값을 가져오는 메서드 (필요한 경우)
    public float GetAttackPower()
    {
        return attackPower;
    }

    // 게임 상태 리셋 메서드 (필요한 경우)
    public void ResetGame()
    {
        if (showDebugLogs)
            Debug.Log("Resetting game state...");

        // 여기에 게임 리셋 로직 추가
    }

#if UNITY_EDITOR
    // 에디터에서 테스트를 위한 메서드들
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }
#endif
}