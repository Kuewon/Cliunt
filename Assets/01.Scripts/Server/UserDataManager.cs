using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;

public class UserDataManager : MonoBehaviour
{
    public static event Action<bool> OnUserDataProcessed;

    [Header("UI Elements")]
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private Slider progressBar;

    [Header("Settings")]
    [SerializeField] private bool showDebugLog = true;

    [Serializable]
    public class UserData
    {
        public string userId;
        public Dictionary<string, object> data;
    }

    private void Awake()
    {
        InitializeGame();
    }

    private void Start()
    {
        if (loadingUI != null) loadingUI.SetActive(true);
        if (lobbyUI != null) lobbyUI.SetActive(false);

        Debug.Log($"📂 유저 데이터 파일 경로: {GetUserDataPath()}");
        GoogleSheetsManager.OnDataLoadComplete += OnGameDataLoaded;
    }

    private void InitializeGame()
    {
        // 기본 성능 설정
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 모바일 환경 최적화
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            QualitySettings.SetQualityLevel(1);
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
        }

        // 초기 메모리 정리
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    private static string GetUserDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "UserData.json");
    }

    private void OnGameDataLoaded()
    {
        if (showDebugLog)
            Debug.Log("✅ 스프레드시트 데이터 로드 완료! 유저 데이터 검증을 시작합니다.");

        bool isNewUser = ProcessUserData();
        OnUserDataProcessed?.Invoke(isNewUser);

        if (loadingUI != null) loadingUI.SetActive(false);
        if (lobbyUI != null) lobbyUI.SetActive(true);
    }

    public static UserData GetCurrentUserData()
    {
        try
        {
            string filePath = GetUserDataPath();
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("⚠️ 유저 데이터 파일이 존재하지 않습니다.");
                return null;
            }

            string jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<UserData>(jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 유저 데이터 로드 실패: {e.Message}");
            return null;
        }
    }

    public static void SaveUserData(UserData userData)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.None
            };

            string jsonData = JsonConvert.SerializeObject(userData, settings);
            File.WriteAllText(GetUserDataPath(), jsonData);
            
            Debug.Log("✅ 유저 데이터 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 유저 데이터 저장 실패: {e.Message}");
        }
    }

    private bool ProcessUserData()
    {
        bool isNewUser = !File.Exists(GetUserDataPath());

        try
        {
            if (isNewUser)
            {
                Debug.Log("🚀 신규 유저 데이터 생성을 시작합니다.");
                CreateUserDataFromLocalSettings();
            }
            else
            {
                Debug.Log("✅ 기존 유저 데이터 검증을 시작합니다.");
                UpdateUserDataWithNewFields();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 유저 데이터 처리 중 오류 발생: {e.Message}");
        }

        return isNewUser;
    }

    private void CreateUserDataFromLocalSettings()
    {
        Dictionary<string, object> localSettings = GameData.Instance.GetRow("UserLocalBaseSetting", 0);

        if (localSettings == null || localSettings.Count == 0)
        {
            Debug.LogError("❌ `UserLocalBaseSetting` 데이터를 찾을 수 없습니다!");
            return;
        }

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

        string directory = Path.GetDirectoryName(GetUserDataPath());
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        SaveUserData(newUser);
        Debug.Log($"✅ 새 유저 데이터 생성 완료!\n📂 저장 위치: {GetUserDataPath()}");
    }

    private void UpdateUserDataWithNewFields()
    {
        UserData existingUser = GetCurrentUserData();
        Dictionary<string, object> localSettings = GameData.Instance.GetRow("UserLocalBaseSetting", 0);

        if (localSettings == null || localSettings.Count == 0)
        {
            Debug.LogError("❌ `UserLocalBaseSetting` 데이터를 찾을 수 없습니다!");
            return;
        }

        bool updated = false;
        foreach (var kvp in localSettings)
        {
            if (!existingUser.data.ContainsKey(kvp.Key))
            {
                existingUser.data[kvp.Key] = kvp.Value;
                updated = true;
                Debug.Log($"🔄 새로운 필드 추가: {kvp.Key}");
            }
        }

        if (updated)
        {
            SaveUserData(existingUser);
            Debug.Log("✅ 유저 데이터 업데이트 완료");
        }
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= OnGameDataLoaded;
    }

    // 게임 씬으로 전환
    public void StartGame()
    {
        if (loadingUI != null) loadingUI.SetActive(true);
        if (lobbyUI != null) lobbyUI.SetActive(false);

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScene").completed += (op) =>
        {
            if (loadingUI != null) loadingUI.SetActive(false);
        };
    }
}