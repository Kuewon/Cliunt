using System;
using UnityEngine;
using Newtonsoft.Json;

public class EquipmentManager : MonoBehaviour
{
    #region Singleton
    public static EquipmentManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
            
            // ✅ 이벤트 구독을 Awake()에서 처리
            UserDataManager.OnUserDataProcessed += Initialize;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Variables
    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private UserDataManager userDataManager;
    private int equippedRevolverIndex = 0;

    
    #endregion

    #region Unity Events
    private void OnDestroy()
    {
        UserDataManager.OnUserDataProcessed -= Initialize;
    }
    #endregion

    #region Initialization
    private void InitializeManager()
    {
        userDataManager = FindObjectOfType<UserDataManager>();
        if (userDataManager == null && showDebugLog)
        {
            Debug.LogWarning("⚠️ UserDataManager를 찾을 수 없습니다!");
        }
    }

    private void Initialize(bool isNewUser)
    {
        if (showDebugLog)
        {
            Debug.Log($"🎮 EquipmentManager 초기화 - 신규 유저: {isNewUser}");
        }
        LoadRevolverData();
    }
    #endregion

    #region Equipment Operations
    public void EquipRevolver(int index)
    {
        LogEquipmentChange("리볼버", equippedRevolverIndex, index);
        equippedRevolverIndex = index;
        SaveRevolverData();
        
        // ✅ UI 자동 업데이트
        FindObjectOfType<EquipmentUI>()?.UpdateRevolverUI();
    }
    #endregion

    #region Data Management
    private void LoadRevolverData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            equippedRevolverIndex = Convert.ToInt32(userData.data[
                "playerRevolverIndex"]);
            
            LogLoadSuccess();

            // ✅ UI 업데이트 추가
            FindObjectOfType<EquipmentUI>()?.UpdateRevolverUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 리볼버 데이터 로드 실패: {e.Message}");
        }
    }

    private void SaveRevolverData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            userData.data["playerRevolverIndex"] = equippedRevolverIndex;
            UserDataManager.SaveUserData(userData);
            
            if (showDebugLog)
                Debug.Log("✅ 리볼버 데이터 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 리볼버 데이터 저장 실패: {e.Message}");
        }
    }
    #endregion

    #region Getters
    public int GetEquippedRevolverIndex() => equippedRevolverIndex;
    #endregion

    #region Debug Helpers
    private void LogEquipmentChange(string equipmentType, int oldIndex, int newIndex)
    {
        if (showDebugLog)
            Debug.Log($"🔄 {equipmentType} 장착: {oldIndex} → {newIndex}");
    }

    private void LogLoadSuccess()
    {
        if (showDebugLog)
            Debug.Log($"✅ 리볼버 데이터 로드 완료! 장착된 리볼버: {equippedRevolverIndex}");
    }
    #endregion
}