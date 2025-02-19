using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int totalGold = 0;
    public int totalDiamonds = 0;
    public int stage = 1;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI diamondText;
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Stage Transition")]
    [SerializeField] private GameObject transitionUI;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string animationTriggerName = "Play";
    private CanvasGroup transitionCanvasGroup;
    private bool transitionCompleted = false;

    private void Awake()
    {
        // Load saved values
        totalGold = (int)PlayerPrefs.GetFloat("USER_GOLD", 0);
        totalDiamonds = (int)PlayerPrefs.GetFloat("USER_DIA", 0);
        stage = (int)PlayerPrefs.GetFloat("USER_STAGE", 1);

        // 트랜지션 UI 초기화
        if (transitionUI != null)
        {
            transitionCanvasGroup = transitionUI.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup)
            {
                transitionCanvasGroup.alpha = 0f;
                transitionUI.SetActive(false);
            }
        }

        // Update UI
        UpdateGoldText();
        UpdateDiamondText();
        UpdateStageText();
    }

    void Start()
    {
        if (WaveManager.Instance != null)
        {
            // 저장된 스테이지로 시작
            // WaveManager.Instance.StartStage(stage);

            WaveManager.Instance.OnStageChanged += OnStageChanged;
            WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;

            // 초기 UI 업데이트
            UpdateStageText();
        }
    }

    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.SetText(totalGold.ToString());
        }
    }

    private void UpdateDiamondText()
    {
        if (diamondText != null)
        {
            diamondText.SetText(totalDiamonds.ToString());
        }
    }

    private void UpdateStageText()
    {
        if (stageText != null)
        {
            stageText.SetText($"Stage : {stage}");
        }
    }

    public void OnUpdateGold(int gold)
    {
        totalGold += gold;
        UpdateGoldText();
        PlayerPrefs.SetFloat("USER_GOLD", totalGold);
    }

    public void OnUpdateDiamonds(int diamonds)
    {
        totalDiamonds += diamonds;
        UpdateDiamondText();
        PlayerPrefs.SetFloat("USER_DIA", totalDiamonds);
    }

    public void OnUpdateStage(int stage)
    {
        this.stage = stage;
        UpdateStageText();
        PlayerPrefs.SetFloat("USER_STAGE", stage);
    }

    private void OnStageChanged(int newStage)
    {
        OnUpdateStage(newStage);

        // WaveManager에서 트랜지션 재생 여부 확인
        if (WaveManager.Instance.ShouldPlayStageTransition())
        {
            StartCoroutine(PlayStageTransition());
        }
    }

    private void OnWaveCompleted()
    {
        // 스테이지의 마지막 웨이브인지 확인
        if (WaveManager.Instance != null &&
            WaveManager.Instance.IsCurrentWaveComplete() &&
            !WaveManager.Instance.HasNextWave() &&
            !WaveManager.Instance.IsStageTransitioning)
        {
            var stageData = GameData.Instance.GetRow("WaveInfo", WaveManager.Instance.GetCurrentStage() - 1);
            if (stageData != null && stageData.ContainsKey("stageClearDia"))
            {
                int clearDiamonds = System.Convert.ToInt32(stageData["stageClearDia"]);
                OnUpdateDiamonds(clearDiamonds);
            }
        }
    }

    private IEnumerator PlayStageTransition()
    {
        if (transitionUI != null && transitionAnimator && transitionCanvasGroup)
        {
            // 트랜지션 시작
            transitionCompleted = false;
            transitionUI.SetActive(true);
            transitionCanvasGroup.alpha = 1f;
            transitionAnimator.SetTrigger(animationTriggerName);

            // 애니메이션이 끝날 때까지 대기
            while (!transitionCompleted)
            {
                yield return null;
            }

            // 트랜지션 종료
            transitionCanvasGroup.alpha = 0f;
            transitionUI.SetActive(false);
        }
    }

    // 애니메이션 이벤트를 통해 호출될 메서드
    public void OnTransitionAnimationComplete()
    {
        transitionCompleted = true;
    }

    void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnStageChanged -= OnStageChanged;
            WaveManager.Instance.OnWaveCompleted -= OnWaveCompleted;
        }
    }
}