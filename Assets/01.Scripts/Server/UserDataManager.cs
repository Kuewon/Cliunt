using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class UserDataManager : MonoBehaviour
{
    public static event Action<bool> OnUserDataProcessed; // 유저 데이터 처리 완료 이벤트

    private static string GetUserDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "UserData.json");
    }

    [Serializable]
    public class UserData
    {
        public string userId;
        public Dictionary<string, object> data;
    }

    private void Start()
    {
        Debug.Log($"📂 유저 데이터 파일 경로: {GetUserDataPath()}");
        GoogleSheetsManager.OnDataLoadComplete += OnGameDataLoaded;
    }

    private void OnGameDataLoaded()
    {
        Debug.Log("✅ Google 스프레드시트 데이터 로드 완료! 유저 데이터 검증을 시작합니다.");

        bool isNewUser = ProcessUserData();
        OnUserDataProcessed?.Invoke(isNewUser);
    }

    public static UserData GetCurrentUserData()
    {
        string jsonData = File.ReadAllText(GetUserDataPath());
        return JsonConvert.DeserializeObject<UserData>(jsonData);
    }

    public static void SaveUserData(UserData userData)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None  // 타입 정보를 제외하고 저장
        };

        string jsonData = JsonConvert.SerializeObject(userData, settings);
        File.WriteAllText(GetUserDataPath(), jsonData);
    }

    private bool ProcessUserData()
    {
        bool isNewUser = !File.Exists(GetUserDataPath());

        if (isNewUser)
        {
            Debug.Log("🚀 유저 데이터가 존재하지 않습니다. `UserLocalBaseSetting`을 기반으로 새 유저 생성.");
            CreateUserDataFromLocalSettings();
        }
        else
        {
            Debug.Log("✅ 유저 데이터가 존재합니다. 데이터 검증을 진행합니다.");
            UpdateUserDataWithNewFields();
        }

        OnUserDataProcessed?.Invoke(isNewUser);
        return isNewUser;
    }

    private void CreateUserDataFromLocalSettings()
    {
        Dictionary<string, object> localSettings = GameData.Instance.GetRow("UserLocalBaseSetting", 0);

        if (localSettings == null || localSettings.Count == 0)
        {
            Debug.LogError("❌ `UserLocalBaseSetting` 데이터를 찾을 수 없습니다!");
            localSettings = new Dictionary<string, object>();
        }

        // 필수 기본값들 확인 및 설정
        if (!localSettings.ContainsKey("playerRevolverIndex")) localSettings["playerRevolverIndex"] = 0;
        if (!localSettings.ContainsKey("playerCylinderIndex")) localSettings["playerCylinderIndex"] = 0;
        if (!localSettings.ContainsKey("playerBulletIndex")) localSettings["playerBulletIndex"] = 0;

        // 배열 데이터 처리
        string[] gradeTypes = { "Revolver", "Cylinder", "Bullet" };
        foreach (var type in gradeTypes)
        {
            for (int grade = 0; grade <= 5; grade++)
            {
                string key = $"has_{grade}grade_{type}";
                int[] arrayData = GameData.Instance.GetArray<int>("UserLocalBaseSetting", 0, key, new int[] { 0, 0, 0, 0 });
                localSettings[key] = arrayData;
            }
        }

        UserData newUser = new UserData
        {
            userId = Guid.NewGuid().ToString(),
            data = localSettings
        };

        SaveUserData(newUser);
        Debug.Log($"✅ 새 유저 데이터 생성 완료!\n📂 저장 위치: {GetUserDataPath()}");
    }

    private void UpdateUserDataWithNewFields()
    {
        string filePath = GetUserDataPath();
        if (!File.Exists(filePath))
        {
            Debug.LogError("❌ 유저 데이터 파일이 존재하지 않습니다. 새로 생성해야 합니다.");
            return;
        }

        UserData existingUser = GetCurrentUserData();
        Dictionary<string, object> localSettings = GameData.Instance.GetRow("UserLocalBaseSetting", 0);
        
        if (localSettings == null || localSettings.Count == 0)
        {
            Debug.LogError("❌ `UserLocalBaseSetting` 데이터를 찾을 수 없습니다! 기존 데이터 유지.");
            return;
        }

        bool updated = false;
        foreach (var kvp in localSettings)
        {
            if (!existingUser.data.ContainsKey(kvp.Key))
            {
                existingUser.data[kvp.Key] = kvp.Value;
                updated = true;
                Debug.Log($"🔄 새로운 항목 추가됨: {kvp.Key} = {kvp.Value}");
            }
        }

        if (updated)
        {
            SaveUserData(existingUser);
            Debug.Log("✅ 유저 데이터가 업데이트되었습니다.");
        }
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= OnGameDataLoaded;
    }
    
    
}