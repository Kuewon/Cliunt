using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public static GameData Instance { get; private set; } = new GameData();

    private Dictionary<string, List<Dictionary<string, object>>> sheetData = new Dictionary<string, List<Dictionary<string, object>>>();

    /// <summary>
    /// 특정 시트의 데이터를 저장
    /// </summary>
    public void SetSheetData(string sheetName, List<Dictionary<string, object>> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning($"⚠️ `{sheetName}` 시트의 데이터가 비어 있습니다.");
            return;
        }

        sheetData[sheetName] = data;
        Debug.Log($"✅ `{sheetName}` 시트 데이터 저장 완료! 총 {data.Count}개의 행이 저장되었습니다.");
    }

    /// <summary>
    /// 특정 시트에서 특정 인덱스의 데이터 가져오기
    /// </summary>
    public Dictionary<string, object> GetRow(string sheetName, int index)
    {
        if (!sheetData.ContainsKey(sheetName))
        {
            Debug.LogError($"❌ `{sheetName}` 시트를 찾을 수 없습니다.");
            return null;
        }

        if (index < 0 || index >= sheetData[sheetName].Count)
        {
            Debug.LogError($"❌ `{sheetName}` 시트에서 인덱스 {index}를 찾을 수 없습니다. (총 {sheetData[sheetName].Count}개의 행 존재)");
            return null;
        }
        return sheetData[sheetName][index];
    }

    /// <summary>
    /// 특정 시트, 특정 인덱스, 특정 키의 데이터 가져오기
    /// </summary>
    public object GetValue(string sheetName, int index, string key)
    {
        Dictionary<string, object> row = GetRow(sheetName, index);
        if (row == null)
        {
            Debug.LogError($"❌ `{sheetName}` 시트에서 인덱스 {index}의 데이터를 찾을 수 없습니다.");
            return null;
        }

        if (!row.ContainsKey(key))
        {
            Debug.LogError($"❌ `{sheetName}` 시트에서 인덱스 {index}에 `{key}` 키가 존재하지 않습니다.");
            return null;
        }
        return row[key];
    }
}