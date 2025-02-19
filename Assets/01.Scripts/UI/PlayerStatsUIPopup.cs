using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // DOTween 네임스페이스 추가

public class PlayerStatsUIPopup : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject dimObject;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;
    [SerializeField] private TMP_Text attackRangeText;
    [SerializeField] private TMP_Text criticalChanceText;
    [SerializeField] private TMP_Text criticalMultiplierText;
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.4f; // 팝업 애니메이션 시간
    [SerializeField] private Vector3 popupStartScale = new Vector3(0.7f, 0.7f, 1f); // 시작 크기
    [SerializeField] private float popupEndScale = 1.0f; // 최종 크기

    private void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("⚠️ PlayerController를 찾을 수 없습니다!");
            }
        }

        // X 버튼에 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseStatsPanel);
        }
        else
        {
            Debug.LogWarning("⚠️ Close Button이 할당되지 않았습니다!");
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
            statsPanel.transform.localScale = popupStartScale;
        }

        if (dimObject != null)
        {
            dimObject.SetActive(false);
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
        if (playerController == null)
        {
            Debug.LogWarning("⚠️ PlayerController를 찾을 수 없어 스탯을 표시할 수 없습니다.");
            return;
        }

        // PlayerController로부터 현재 스탯 가져오기
        float attackDamage = playerController.GetAttackDamage();
        float attackSpeed = playerController.GetAttackSpeed();
        float attackRange = playerController.GetAttackRange();
        float criticalChance = playerController.GetCriticalChance();
        float criticalMultiplier = playerController.GetCriticalMultiplier();

        // UI 텍스트 업데이트
        attackDamageText.text = $"공격력: {attackDamage:F1}";
        attackSpeedText.text = $"공격 속도: {attackSpeed:F2}/s";
        attackRangeText.text = $"사거리: {attackRange:F1}";
        criticalChanceText.text = $"치명타 확률: {criticalChance * 100:F1}%";
        criticalMultiplierText.text = $"치명타 배율: x{criticalMultiplier:F1}";
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
    
    private void OnDestroy()
    {
        // 이벤트 리스너 제거
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseStatsPanel);
        }
    }
}