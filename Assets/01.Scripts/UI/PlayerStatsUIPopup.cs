using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerStatsUIPopup : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject statsPanel; // 팝업 패널
    [SerializeField] private GameObject dimObject; // 딤(배경 어두운 효과)
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;
    [SerializeField] private TMP_Text attackRangeText;
    [SerializeField] private TMP_Text criticalChanceText;
    [SerializeField] private TMP_Text criticalMultiplierText;

    private void Start()
    {
        if (statsPanel != null) statsPanel.SetActive(false); // 시작 시 숨김
        if (dimObject != null) dimObject.SetActive(false); // Dim도 숨김
    }

    public void ToggleStatsPanel()
    {
        bool isActive = !statsPanel.activeSelf;
        statsPanel.SetActive(isActive);
        if (dimObject != null) dimObject.SetActive(isActive); // Dim 표시

        if (isActive)
        {
            UpdateStatsUI();
        }
    }

    public void CloseStatsPanel()
    {
        statsPanel.SetActive(false);
        if (dimObject != null) dimObject.SetActive(false); // Dim도 닫기
    }

    private void UpdateStatsUI()
    {
        var statsData = GameData.Instance.GetRow("PlayerStats", 0);
        if (statsData == null)
        {
            Debug.LogError("⚠️ 플레이어 스탯 데이터를 찾을 수 없습니다.");
            return;
        }

        attackDamageText.text = $"공격력: {GetStatValue(statsData, "baseAttackDamage")}";
        attackSpeedText.text = $"공격 속도: {GetStatValue(statsData, "baseAttackSpeed")}";
        attackRangeText.text = $"사거리: {GetStatValue(statsData, "attackRange")}";
        criticalChanceText.text = $"치명타 확률: {GetStatValue(statsData, "criticalChance") * 100}%";
        criticalMultiplierText.text = $"치명타 배율: x{GetStatValue(statsData, "criticalDamageMultiplier")}";
    }

    private float GetStatValue(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out object value) && value != null)
        {
            if (float.TryParse(value.ToString(), out float result))
                return result;
        }
        return 0f;
    }
}