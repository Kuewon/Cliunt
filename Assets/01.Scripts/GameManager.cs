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
    public GameObject damageTextObj;

    private void Awake()
    {
        // Load saved values
        totalGold = (int)PlayerPrefs.GetFloat("USER_GOLD", 0);
        totalDiamonds = (int)PlayerPrefs.GetFloat("USER_DIA", 0);
        stage = (int)PlayerPrefs.GetFloat("USER_STAGE", 1);

        // Update UI
        UpdateGoldText();
        UpdateDiamondText();
        UpdateStageText();
    }

    void Start()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnStageChanged += OnUpdateStage;
            WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
            OnUpdateStage(WaveManager.Instance.GetCurrentStage());
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
    
    void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnStageChanged -= OnUpdateStage;
            WaveManager.Instance.OnWaveCompleted -= OnWaveCompleted;
        }
    }
}