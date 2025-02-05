using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class GoogleSheetsManager : MonoBehaviour
{
    private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    private static readonly string ApplicationName = "Unity Google Sheets Integration";
    private static readonly string SpreadsheetId = "1VyuylG7ABCyohVL_3u1fMNcNEMjCAK3acCOgf42K-3c";

    private SheetsService service;

    private void Start()
    {
        AuthenticateGoogleSheets();
        List<string> loadedSheets = LoadAllSheets();

        // 콘솔에 보기 쉽게 시트 개수 및 줄 단위 목록 출력
        Debug.Log($"✅ Google Sheets에서 {loadedSheets.Count}개의 시트를 불러왔습니다:\n" + FormatSheetList(loadedSheets));
    }

    private void AuthenticateGoogleSheets()
    {
        string jsonPath = Path.Combine(Application.persistentDataPath, "tough-forest-450011-r5-9f21fbd2257a.json");

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("❌ JSON Key File not found: " + jsonPath);
            return;
        }

        GoogleCredential credential;
        using (var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    private List<string> LoadAllSheets()
    {
        var request = service.Spreadsheets.Get(SpreadsheetId);
        Spreadsheet spreadsheet = request.Execute();
        List<string> sheetNames = new List<string>();

        foreach (var sheet in spreadsheet.Sheets)
        {
            string sheetName = sheet.Properties.Title;
            if (sheetName.StartsWith("@")) continue;

            LoadSheetData(sheetName);
            sheetNames.Add(sheetName);
        }

        return sheetNames;
    }

    private void LoadSheetData(string sheetName)
    {
        string range = $"{sheetName}!A1:Z";
        var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        ValueRange response = request.Execute();

        if (response.Values == null || response.Values.Count < 4)
        {
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

    private object ConvertToType(string type, string value)
    {
        switch (type)
        {
            case "int":
                return int.TryParse(value, out int intValue) ? intValue : 0;
            case "float":
                return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue) ? floatValue : 0f;
            case "string":
                return value;
        }
        return null;
    }

    /// <summary>
    /// 시트 목록을 "1: PlayerStats", "2: Currency" 형식으로 변환하여 줄 단위로 반환
    /// </summary>
    private string FormatSheetList(List<string> sheets)
    {
        string formattedList = "";
        for (int i = 0; i < sheets.Count; i++)
        {
            formattedList += $"{i + 1}: {sheets[i]}\n";
        }
        return formattedList.TrimEnd(); // 마지막 줄 바꿈 제거
    }
}