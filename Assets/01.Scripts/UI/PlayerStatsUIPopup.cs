using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening; // DOTween 네임스페이스 추가

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

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.4f; // 팝업 애니메이션 시간
    [SerializeField] private Vector3 popupStartScale = new Vector3(0.7f, 0.7f, 1f); // 시작 크기
    [SerializeField] private float popupEndScale = 1.0f; // 최종 크기

    private void Start()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(false); // 시작 시 숨김
            statsPanel.transform.localScale = popupStartScale; // 초기 크기 설정
        }

        if (dimObject != null)
        {
            dimObject.SetActive(false); // 🔹 dim은 단순히 껐다 켜는 용도
        }
    }

    public void ToggleStatsPanel()
    {
        bool isActive = !statsPanel.activeSelf;
        if (isActive)
        {
            ShowPopup();
        }
        else
        {
            CloseStatsPanel();
        }
    }

    private void ShowPopup()
    {
        if (statsPanel == null) return;

        // 🔹 dim을 단순 활성화 (애니메이션 없음)
        if (dimObject != null)
        {
            dimObject.SetActive(true);
        }

        // 🔹 팝업 애니메이션 적용
        statsPanel.SetActive(true);
        statsPanel.transform.localScale = popupStartScale;
        statsPanel.transform.DOScale(popupEndScale, animationDuration)
            .SetEase(Ease.OutBack); // 부드러운 확대 애니메이션

        UpdateStatsUI(); // 데이터 업데이트
    }

    public void CloseStatsPanel()
    {
        if (statsPanel == null) return;

        // 🔹 DOTween 없이 즉시 비활성화
        statsPanel.SetActive(false);

        // 🔹 dim도 즉시 비활성화
        if (dimObject != null)
        {
            dimObject.SetActive(false);
        }
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