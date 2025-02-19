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
    private SpinnerController spinnerController;
    private PlayerController playerController;
    private int equippedRevolverIndex = 0;
    private int equippedCylinderIndex = 0;
    private int equippedBulletIndex = 0;
    private int[] equippedBullets = new int[6];
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

        // 추가: SpinnerController 찾기
        UpdateSpinnerControllerReference();
        UpdatePlayerControllerReference();
    }

    // 추가: SpinnerController 참조 업데이트 메서드
    private void UpdateSpinnerControllerReference()
    {
        spinnerController = FindObjectOfType<SpinnerController>();
        if (spinnerController == null && showDebugLog)
        {
            Debug.LogWarning("⚠️ SpinnerController를 찾을 수 없습니다!");
        }
    }
    
    private void UpdatePlayerControllerReference()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null && showDebugLog)
        {
            Debug.LogWarning("⚠️ PlayerController를 찾을 수 없습니다!");
        }
    }

    private void Initialize(bool isNewUser)
    {
        if (showDebugLog)
        {
            Debug.Log($"🎮 EquipmentManager 초기화 - 신규 유저: {isNewUser}");
        }
        LoadRevolverData();
        LoadCylinderData();
        LoadBulletData();
        
        // 추가: 실린더 데이터 로드 후 스피너 업데이트
        UpdateSpinnerMaxSpeed();
    }
    #endregion

    #region Equipment Operations
    public void EquipRevolver(int index)
    {
        LogEquipmentChange("리볼버", equippedRevolverIndex, index);
        equippedRevolverIndex = index;
        SaveRevolverData();
        
        FindObjectOfType<EquipmentUI>()?.UpdateRevolverUI();

        // 플레이어 스탯 업데이트
        if (playerController == null)
        {
            UpdatePlayerControllerReference();
        }

        if (playerController != null)
        {
            playerController.InitializeStats();
            if (showDebugLog)
            {
                Debug.Log("✅ 리볼버 장착으로 인한 플레이어 스탯 업데이트 완료");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerController를 찾을 수 없어 스탯 업데이트를 할 수 없습니다!");
        }
    }
    
    public void EquipCylinder(int index)
    {
        LogEquipmentChange("실린더", equippedCylinderIndex, index);
        equippedCylinderIndex = index;
        SaveCylinderData();
        
        FindObjectOfType<EquipmentUI>()?.UpdateCylinderUI();
        
        // 추가: 실린더 장착 시 스피너 업데이트
        UpdateSpinnerMaxSpeed();
    }
    
    public void EquipBullet(int index)
    {
        // 모든 슬롯에 동일한 총알 장착
        for (int i = 0; i < 6; i++)
        {
            equippedBullets[i] = index;
        }
        
        if (showDebugLog)
        {
            Debug.Log($"🎯 모든 슬롯에 총알 {index} 장착 완료");
        }
        
        SaveBulletData();
        FindObjectOfType<EquipmentUI>()?.UpdateBulletUI();
    }

    // 추가: 스피너 MaxSpeed 업데이트 메서드
    private void UpdateSpinnerMaxSpeed()
    {
        if (spinnerController == null)
        {
            UpdateSpinnerControllerReference();
        }

        if (spinnerController != null)
        {
            spinnerController.OnCylinderEquipped();
            if (showDebugLog)
            {
                Debug.Log($"🎡 스피너 최대 속도 업데이트 완료 (실린더 인덱스: {equippedCylinderIndex})");
            }
        }
    }
    #endregion

    #region Data Management
    private void LoadRevolverData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            equippedRevolverIndex = Convert.ToInt32(userData.data["playerRevolverIndex"]);
            
            LogLoadSuccess();
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
    
    private void LoadCylinderData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            equippedCylinderIndex = Convert.ToInt32(userData.data["playerCylinderIndex"]);
            
            if (showDebugLog)
                Debug.Log($"✅ 실린더 데이터 로드 완료! 장착된 실린더: {equippedCylinderIndex}");

            FindObjectOfType<EquipmentUI>()?.UpdateCylinderUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 실린더 데이터 로드 실패: {e.Message}");
        }
    }
    
    private void SaveCylinderData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            userData.data["playerCylinderIndex"] = equippedCylinderIndex;
            UserDataManager.SaveUserData(userData);
            
            if (showDebugLog)
                Debug.Log("✅ 실린더 데이터 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 실린더 데이터 저장 실패: {e.Message}");
        }
    }
    
    private void SaveBulletData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            // 6개 총알 데이터를 JSON 문자열로 저장
            userData.data["playerBulletIndices"] = JsonConvert.SerializeObject(equippedBullets);
            UserDataManager.SaveUserData(userData);
        
            if (showDebugLog)
            {
                Debug.Log($"✅ 총알 데이터 저장 완료 (모든 슬롯: {equippedBullets[0]})");
            }

            // SpinnerController 참조 확인 및 업데이트
            if (spinnerController == null)
            {
                UpdateSpinnerControllerReference();
            }

            if (spinnerController != null)
            {
                spinnerController.UpdateBullettFromEquipment();
                if (showDebugLog)
                {
                    Debug.Log("✅ SpinnerController 총알 상태 업데이트 완료");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ SpinnerController를 찾을 수 없어 총알 상태를 업데이트할 수 없습니다!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 총알 데이터 저장 실패: {e.Message}");
        }
    }
    
    private void LoadBulletData()
    {
        try
        {
            var userData = UserDataManager.GetCurrentUserData();
            if (userData?.data == null) return;

            if (userData.data.ContainsKey("playerBulletIndices"))
            {
                equippedBullets = JsonConvert.DeserializeObject<int[]>(userData.data["playerBulletIndices"].ToString());
            }
            else
            {
                // 데이터가 없는 경우 모든 슬롯을 0으로 초기화
                for (int i = 0; i < 6; i++)
                {
                    equippedBullets[i] = 0;
                }
            }
            
            if (showDebugLog)
            {
                Debug.Log($"✅ 총알 데이터 로드 완료! 장착된 총알: {equippedBullets[0]}");
            }

            FindObjectOfType<EquipmentUI>()?.UpdateBulletUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 총알 데이터 로드 실패: {e.Message}");
        }
    }
    #endregion

    #region Getters
    public int GetEquippedRevolverIndex() => equippedRevolverIndex;
    public int GetEquippedCylinderIndex() => equippedCylinderIndex;
    public int GetEquippedBulletIndex() => equippedBulletIndex; 
    public int[] GetEquippedBullets() => equippedBullets;
    public int GetCurrentBulletIndex() => equippedBullets[0];
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