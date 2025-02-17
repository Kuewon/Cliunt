using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class GoogleSheetsManager : MonoBehaviour
{
   public static event Action OnDataLoadComplete; // 데이터 로드 완료 이벤트

   private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
   private static readonly string ApplicationName = "Unity Google Sheets Integration";
   private static readonly string SpreadsheetId = "1VyuylG7ABCyohVL_3u1fMNcNEMjCAK3acCOgf42K-3c";

   private SheetsService service;

   private async void Start()
   {
       await AuthenticateGoogleSheets();
       LoadAllSheets();
   }

   /// <summary>
   /// Google Sheets API 인증
   /// </summary>
   private async Task AuthenticateGoogleSheets()
   {
       string jsonContent = await LoadJsonFile();
       if (string.IsNullOrEmpty(jsonContent))
       {
           Debug.LogError("❌ 인증 파일 로드 실패");
           return;
       }

       GoogleCredential credential;
       try
       {
           using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent)))
           {
               credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
           }

           service = new SheetsService(new BaseClientService.Initializer()
           {
               HttpClientInitializer = credential,
               ApplicationName = ApplicationName,
           });

           Debug.Log("✅ Google Sheets API 인증 성공!");
       }
       catch (Exception e)
       {
           Debug.LogError($"❌ Google Sheets API 인증 중 오류 발생: {e.Message}");
       }
   }

   /// <summary>
   /// 플랫폼에 따른 인증 파일 로드
   /// </summary>
   private async Task<string> LoadJsonFile()
   {
       const string fileName = "tough-forest-450011-r5-9f21fbd2257a.json";
       
       if (Application.platform == RuntimePlatform.Android)
       {
           string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
           using (UnityWebRequest www = UnityWebRequest.Get(filePath))
           {
               try
               {
                   var operation = www.SendWebRequest();
                   while (!operation.isDone)
                       await Task.Yield();

                   if (www.result == UnityWebRequest.Result.Success)
                   {
                       return www.downloadHandler.text;
                   }
                   else
                   {
                       Debug.LogError($"❌ 인증 파일 다운로드 실패: {www.error}");
                       return null;
                   }
               }
               catch (Exception e)
               {
                   Debug.LogError($"❌ 인증 파일 로드 중 오류 발생: {e.Message}");
                   return null;
               }
           }
       }
       else
       {
           string jsonPath = Path.Combine(Application.persistentDataPath, fileName);
           if (!File.Exists(jsonPath))
           {
               Debug.LogError($"❌ JSON Key File not found: {jsonPath}");
               return null;
           }
           return File.ReadAllText(jsonPath);
       }
   }

   /// <summary>
   /// 모든 시트 데이터를 로드
   /// </summary>
   private void LoadAllSheets()
   {
       if (service == null) 
       {
           Debug.LogError("❌ Google Sheets API 서비스가 초기화되지 않았습니다. 인증이 먼저 필요합니다.");
           return;
       }

       var request = service.Spreadsheets.Get(SpreadsheetId);
       Spreadsheet spreadsheet;

       try
       {
           spreadsheet = request.Execute();
       }
       catch (Exception e)
       {
           Debug.LogError($"❌ Google Sheets 데이터를 불러오는 중 오류 발생: {e.Message}");
           return;
       }

       List<string> sheetNames = new List<string>();

       foreach (var sheet in spreadsheet.Sheets)
       {
           string sheetName = sheet.Properties.Title;
           if (sheetName.StartsWith("@")) continue;

           LoadSheetData(sheetName);
           sheetNames.Add(sheetName);
       }

       Debug.Log($"✅ Google 스프레드시트 데이터 로드 완료! 총 {sheetNames.Count}개의 시트를 불러왔습니다.");
       foreach (var name in sheetNames)
       {
           Debug.Log($"📄 {name}");
       }

       OnDataLoadComplete?.Invoke(); // ✅ 데이터 로드 완료 이벤트 호출
   }

   /// <summary>
   /// 개별 시트 데이터를 불러와 GameData에 저장
   /// </summary>
   private void LoadSheetData(string sheetName)
   {
       string range = $"{sheetName}!A1:ZZ";
       var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
       ValueRange response;

       try
       {
           response = request.Execute();
       }
       catch (Exception e)
       {
           Debug.LogError($"❌ {sheetName} 시트 데이터를 불러오는 중 오류 발생: {e.Message}");
           return;
       }

       if (response.Values == null || response.Values.Count < 4)
       {
           Debug.LogWarning($"⚠️ '{sheetName}' 시트에 유효한 데이터가 없습니다.");
           return;
       }

       List<string> columnTypes = new List<string>();
       foreach (var cell in response.Values[0])
       {
           columnTypes.Add(cell.ToString().ToLower());
       }

       List<string> columnNames = new List<string>();
       foreach (var cell in response.Values[1])
       {
           columnNames.Add(cell.ToString());
       }

       List<Dictionary<string, object>> sheetData = new List<Dictionary<string, object>>();

       for (int i = 3; i < response.Values.Count; i++)
       {
           var row = response.Values[i];
           Dictionary<string, object> rowData = new Dictionary<string, object>();

           for (int j = 0; j < row.Count; j++)
           {
               if (j >= columnNames.Count) continue;

               string key = columnNames[j];
               string type = columnTypes[j];
               string value = row[j]?.ToString();

               rowData[key] = ConvertToType(type, value);
           }

           if (rowData.Count > 0)
           {
               sheetData.Add(rowData);
           }
       }

       GameData.Instance.SetSheetData(sheetName, sheetData);
   }

   /// <summary>
   /// 데이터 타입 변환
   /// </summary>
   private object ConvertToType(string type, string value)
   {
       switch (type)
       {
           case "int":
               return int.TryParse(value, out int intValue) ? intValue : 0;
           case "float":
               return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatValue) ? floatValue : 0f;
           case "string":
               return value;
           case "int[]":
               value = value.Trim();
               if (value.StartsWith("[")) value = value.Substring(1);
               if (value.EndsWith("]")) value = value.Substring(0, value.Length - 1);
               return value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => int.TryParse(x.Trim(), out int num) ? num : 0)
                       .ToArray();
           case "float[]":
               value = value.Trim();
               if (value.StartsWith("[")) value = value.Substring(1);
               if (value.EndsWith("]")) value = value.Substring(0, value.Length - 1);
               return value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => float.TryParse(x.Trim(),
                       System.Globalization.NumberStyles.Float,
                       System.Globalization.CultureInfo.InvariantCulture,
                       out float num) ? num : 0f)
                       .ToArray();
       }
       return null;
   }
}