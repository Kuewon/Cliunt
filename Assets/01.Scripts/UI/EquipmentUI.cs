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
    
    [Header("Cylinder List UI")]
    [SerializeField] private Transform cylinderContentPanel;
    [SerializeField] private GameObject cylinderPrefab;

    [Header("Selected Cylinder UI")]
    [SerializeField] private TMP_Text cylinderNameText;
    [SerializeField] private TMP_Text cylinderGradeText;
    [SerializeField] private TMP_Text cylinderDamageText;
    [SerializeField] private TMP_Text cylinderDescriptionText;
    [SerializeField] private TMP_Text cylinderLevelText;
    [SerializeField] private TMP_Text cylinderEffectText;
    
    [Header("Bullet List UI")]
    [SerializeField] private Transform bulletContentPanel;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Selected Bullet UI")]
    [SerializeField] private TMP_Text bulletNameText;
    [SerializeField] private TMP_Text bulletGradeText;
    [SerializeField] private TMP_Text bulletDamageText;
    [SerializeField] private TMP_Text bulletDescriptionText;
    [SerializeField] private TMP_Text bulletLevelText;
    [SerializeField] private TMP_Text bulletEffectText;

    private void Start()
    {
        LoadRevolverList();
        LoadCylinderList();
        LoadBulletList();
        UpdateRevolverUI();
        UpdateCylinderUI();
        UpdateBulletUI();
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
    
    public void LoadCylinderList()
    {
        string sheetName = "Cylinder";
        List<Dictionary<string, object>> cylinderDataList = GameData.Instance.GetSheet(sheetName);

        if (cylinderDataList == null || cylinderDataList.Count == 0)
        {
            Debug.LogWarning($"⚠️ `{sheetName}` 시트에 데이터가 없습니다.");
            return;
        }

        foreach (Transform child in cylinderContentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var cylinderData in cylinderDataList)
        {
            if (cylinderData == null) continue;

            int index = GameData.Instance.GetInt(sheetName, cylinderDataList.IndexOf(cylinderData), "Index");
            string name = GameData.Instance.GetString(sheetName, cylinderDataList.IndexOf(cylinderData), "cylinderName", "이름 없음");
            int grade = GameData.Instance.GetInt(sheetName, cylinderDataList.IndexOf(cylinderData), "cylinderGrade", 0);

            GameObject newCylinderButton = Instantiate(cylinderPrefab, cylinderContentPanel);
            
            var gradeBackground = newCylinderButton.transform.Find("cylinderEquipmentItem_backgroungIMG")?.GetComponent<Image>();
            if (gradeBackground != null)
            {
                gradeBackground.color = GetGradeColor(grade);
            }

            var buttonText = newCylinderButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = name;
            }

            var button = newCylinderButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => EquipmentManager.Instance.EquipCylinder(index));
            }
        }
    }
    
    public void UpdateCylinderUI()
    {
        int equippedIndex = EquipmentManager.Instance.GetEquippedCylinderIndex();
        string sheetName = "Cylinder";

        var cylinderData = GameData.Instance.GetRow(sheetName, equippedIndex);
        if (cylinderData == null) return;

        string name = GameData.Instance.GetString(sheetName, equippedIndex, "cylinderName", "이름 없음");
        string grade = GetGradeText(GameData.Instance.GetInt(sheetName, equippedIndex, "cylinderGrade", 0));
        float damage = GameData.Instance.GetFloat(sheetName, equippedIndex, "cylinderBaseDamage", 0f);
        string description = GameData.Instance.GetString(sheetName, equippedIndex, "cylinderDescription", "설명 없음");
        int baseLevel = GameData.Instance.GetInt(sheetName, equippedIndex, "cylinderBaseLevel", 1);
        float effect = GameData.Instance.GetFloat(sheetName, equippedIndex, "cylinderBuffEffect", 0f);

        if (cylinderNameText != null) cylinderNameText.text = name;
        if (cylinderGradeText != null) cylinderGradeText.text = $"등급: {grade}";
        if (cylinderDamageText != null) cylinderDamageText.text = $"기본 공격력: {damage:F1}";
        if (cylinderDescriptionText != null) cylinderDescriptionText.text = description;
        if (cylinderLevelText != null) cylinderLevelText.text = $"기본 레벨: {baseLevel}";
        if (cylinderEffectText != null) cylinderEffectText.text = $"버프 효과: {effect:P1}";
    }
    
    public void LoadBulletList()
    {
        string sheetName = "Bullet";
        List<Dictionary<string, object>> bulletDataList = GameData.Instance.GetSheet(sheetName);

        if (bulletDataList == null || bulletDataList.Count == 0)
        {
            Debug.LogWarning($"⚠️ `{sheetName}` 시트에 데이터가 없습니다.");
            return;
        }

        foreach (Transform child in bulletContentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var bulletData in bulletDataList)
        {
            if (bulletData == null) continue;

            int index = GameData.Instance.GetInt(sheetName, bulletDataList.IndexOf(bulletData), "Index");
            string name = GameData.Instance.GetString(sheetName, bulletDataList.IndexOf(bulletData), "bulletName", "이름 없음");
            int grade = GameData.Instance.GetInt(sheetName, bulletDataList.IndexOf(bulletData), "bulletGrade", 0);

            GameObject newBulletButton = Instantiate(bulletPrefab, bulletContentPanel);
            
            var gradeBackground = newBulletButton.transform.Find("bulletEquipmentItem_backgroungIMG")?.GetComponent<Image>();
            if (gradeBackground != null)
            {
                gradeBackground.color = GetGradeColor(grade);
            }

            var buttonText = newBulletButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = name;
            }

            var button = newBulletButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => EquipmentManager.Instance.EquipBullet(index));
            }
        }
    }
    
    public void UpdateBulletUI()
    {
        int equippedIndex = EquipmentManager.Instance.GetEquippedBulletIndex();
        string sheetName = "Bullet";

        var bulletData = GameData.Instance.GetRow(sheetName, equippedIndex);
        if (bulletData == null) return;

        string name = GameData.Instance.GetString(sheetName, equippedIndex, "bulletName", "이름 없음");
        string grade = GetGradeText(GameData.Instance.GetInt(sheetName, equippedIndex, "bulletGrade", 0));
        float damage = GameData.Instance.GetFloat(sheetName, equippedIndex, "bulletBaseDamage", 0f);
        string description = GameData.Instance.GetString(sheetName, equippedIndex, "bulletDescription", "설명 없음");
        int baseLevel = GameData.Instance.GetInt(sheetName, equippedIndex, "bulletBaseLevel", 1);
        float effect = GameData.Instance.GetFloat(sheetName, equippedIndex, "bulletBuffEffect", 0f);

        if (bulletNameText != null) bulletNameText.text = name;
        if (bulletGradeText != null) bulletGradeText.text = $"등급: {grade}";
        if (bulletDamageText != null) bulletDamageText.text = $"기본 공격력: {damage:F1}";
        if (bulletDescriptionText != null) bulletDescriptionText.text = description;
        if (bulletLevelText != null) bulletLevelText.text = $"기본 레벨: {baseLevel}";
        if (bulletEffectText != null) bulletEffectText.text = $"버프 효과: {effect:P1}";
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
    
    private string GetGradeText(int grade)
    {
        return grade switch
        {
            0 => "일반",
            1 => "고급",
            2 => "희귀",
            3 => "영웅",
            4 => "전설",
            5 => "신화",
            _ => "알 수 없음"
        };
    }
}