using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    [Header("Revolver List UI")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject revolverPrefab;

    [Header("Selected Revolver UI")]
    [SerializeField] private TMP_Text revolverNameText;
    [SerializeField] private TMP_Text revolverGradeText;
    [SerializeField] private TMP_Text revolverDamageText;
    [SerializeField] private TMP_Text revolverDescriptionText;
    [SerializeField] private Image revolverImage;

    [Header("Revolver Sprites")]
    [SerializeField] private Sprite[] revolverSprites;

    private void Start()
    {
        LoadRevolverList(); // 전체 리볼버 목록 불러오기
        UpdateRevolverUI(); // 현재 장착한 리볼버 표시
    }

    /// <summary>
    /// Google Sheets에서 "Revolver" 데이터를 가져와 UI 목록에 추가
    /// </summary>
    public void LoadRevolverList()
    {
        string sheetName = "Revolver";
        List<Dictionary<string, object>> revolverDataList = GameData.Instance.GetSheet(sheetName);

        if (revolverDataList == null || revolverDataList.Count == 0)
        {
            Debug.LogWarning($"⚠️ `{sheetName}` 시트에 데이터가 없습니다.");
            return;
        }

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var revolverData in revolverDataList)
        {
            if (revolverData == null) continue;

            int index = GameData.Instance.GetInt(sheetName, revolverDataList.IndexOf(revolverData), "Index");
            string name = GameData.Instance.GetString(sheetName, revolverDataList.IndexOf(revolverData), "revolverName", "이름 없음");
            int grade = GameData.Instance.GetInt(sheetName, revolverDataList.IndexOf(revolverData), "revolverGrade", 0);

            GameObject newRevolverButton = Instantiate(revolverPrefab, contentPanel);
            
            // 프리팹의 특정 이미지 찾기
            var gradeBackground = newRevolverButton.transform.Find("revolverEquipmentItem_backgroungIMG")?.GetComponent<Image>();
            if (gradeBackground != null)
            {
                gradeBackground.color = GetGradeColor(grade);
            }

            var buttonText = newRevolverButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = name;
            }

            var button = newRevolverButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => EquipmentManager.Instance.EquipRevolver(index));
            }
        }
    }

    /// <summary>
    /// 현재 착용한 리볼버 UI 업데이트
    /// </summary>
    public void UpdateRevolverUI()
    {
        int equippedIndex = EquipmentManager.Instance.GetEquippedRevolverIndex();
        string sheetName = "Revolver";

        if (!GameData.Instance.HasRow(sheetName, equippedIndex))
        {
            Debug.LogWarning($"⚠️ `{sheetName}` 시트에 인덱스 {equippedIndex}가 존재하지 않습니다.");
            return;
        }

        // 안전하게 데이터 가져오기
        string name = GameData.Instance.GetString(sheetName, equippedIndex, "revolverName", "이름 없음");
        string grade = GameData.Instance.GetString(sheetName, equippedIndex, "revolverGrade", "일반");
        int damage = GameData.Instance.GetInt(sheetName, equippedIndex, "revolverBaseDamage", 0);
        string description = GameData.Instance.GetString(sheetName, equippedIndex, "revolverDescription", "설명 없음");

        // UI 업데이트
        if (revolverNameText != null) revolverNameText.text = name;
        if (revolverGradeText != null) revolverGradeText.text = $"등급: {grade}";
        if (revolverDamageText != null) revolverDamageText.text = $"공격력: {damage}";
        if (revolverDescriptionText != null) revolverDescriptionText.text = description;

        // 이미지 업데이트
        if (revolverImage != null && revolverSprites != null && revolverSprites.Length > equippedIndex)
        {
            revolverImage.sprite = revolverSprites[equippedIndex];
        }
    }
    
    private Color GetGradeColor(int grade)
    {
        return grade switch
        {
            0 => Color.white,                        // 기본
            1 => new Color(0.4f, 1f, 0.4f, 1f),     // 초록
            2 => new Color(0.4f, 0.7f, 1f, 1f),     // 파랑
            3 => new Color(0.8f, 0.4f, 1f, 1f),     // 보라
            4 => new Color(1f, 0.4f, 0.4f, 1f),     // 빨강
            5 => new Color(1f, 0.9f, 0.4f, 1f),     // 노랑
            _ => Color.white                         // 기본값
        };
    }
}