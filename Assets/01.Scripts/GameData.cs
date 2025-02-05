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
        sheetData[sheetName] = data;
    }

    /// <summary>
    /// 특정 시트에서 특정 인덱스의 데이터 가져오기
    /// </summary>
    public Dictionary<string, object> GetRow(string sheetName, int index)
    {
        if (sheetData.ContainsKey(sheetName) && index >= 0 && index < sheetData[sheetName].Count)
        {
            return sheetData[sheetName][index];
        }
        return null;
    }

    /// <summary>
    /// 특정 시트, 특정 인덱스, 특정 키의 데이터 가져오기
    /// </summary>
    public object GetValue(string sheetName, int index, string key)
    {
        Dictionary<string, object> row = GetRow(sheetName, index);
        return row != null && row.ContainsKey(key) ? row[key] : null;
    }
}