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
    
    /// <summary>
    /// 안전하게 2차원 정수 배열을 가져오는 메서드
    /// </summary>
    public int[][] GetInt2DArray(string sheetName, int index, string key, int[][] defaultValue = null)
    {
        try
        {
            var value = GetValue(sheetName, index, key);
            if (value == null) return defaultValue ?? Array.Empty<int[]>();
            
            if (value is int[][] typedArray) return typedArray;
            if (value is Array arr)
            {
                List<int[]> result = new List<int[]>();
                foreach (var item in arr)
                {
                    if (item is Array innerArr)
                    {
                        int[] innerResult = new int[innerArr.Length];
                        for (int i = 0; i < innerArr.Length; i++)
                        {
                            if (int.TryParse(innerArr.GetValue(i).ToString(), out int num))
                            {
                                innerResult[i] = num;
                            }
                        }
                        result.Add(innerResult);
                    }
                    else if (item is int singleInt)
                    {
                        // 단일 정수인 경우 길이 1의 배열로 처리
                        result.Add(new int[] { singleInt });
                    }
                }
                return result.ToArray();
            }
            
            return defaultValue ?? Array.Empty<int[]>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"⚠️ {sheetName}시트의 {index}행 {key} 2차원 배열 변환 실패. 기본값 반환. 오류: {e.Message}");
            return defaultValue ?? Array.Empty<int[]>();
        }
    }
    
    public List<Dictionary<string, object>> GetSheet(string sheetName)
    {
        if (sheetData.ContainsKey(sheetName))
        {
            return sheetData[sheetName];
        }
        Debug.LogError($"❌ `{sheetName}` 시트 데이터를 찾을 수 없습니다.");
        return new List<Dictionary<string, object>>(); // 빈 리스트 반환 (에러 방지)
    }
    #region Safe Data Access Methods
    /// <summary>
    /// 안전하게 문자열 값을 가져오는 메서드
    /// </summary>
    public string GetString(string sheetName, int index, string key, string defaultValue = "")
    {
        try
        {
            var value = GetValue(sheetName, index, key);
            return value?.ToString() ?? defaultValue;
        }
        catch
        {
            Debug.LogWarning($"⚠️ {sheetName}시트의 {index}행 {key} 문자열 변환 실패. 기본값 반환: {defaultValue}");
            return defaultValue;
        }
    }

    /// <summary>
    /// 안전하게 정수 값을 가져오는 메서드
    /// </summary>
    public int GetInt(string sheetName, int index, string key, int defaultValue = 0)
    {
        try
        {
            var value = GetValue(sheetName, index, key);
            if (value == null) return defaultValue;
            
            if (value is int intValue) return intValue;
            if (int.TryParse(value.ToString(), out int result)) return result;
            
            return defaultValue;
        }
        catch
        {
            Debug.LogWarning($"⚠️ {sheetName}시트의 {index}행 {key} 정수 변환 실패. 기본값 반환: {defaultValue}");
            return defaultValue;
        }
    }

    /// <summary>
    /// 안전하게 float 값을 가져오는 메서드
    /// </summary>
    public float GetFloat(string sheetName, int index, string key, float defaultValue = 0f)
    {
        try
        {
            var value = GetValue(sheetName, index, key);
            if (value == null) return defaultValue;
            
            if (value is float floatValue) return floatValue;
            if (float.TryParse(value.ToString(), out float result)) return result;
            
            return defaultValue;
        }
        catch
        {
            Debug.LogWarning($"⚠️ {sheetName}시트의 {index}행 {key} float 변환 실패. 기본값 반환: {defaultValue}");
            return defaultValue;
        }
    }

    /// <summary>
    /// 안전하게 배열 값을 가져오는 메서드
    /// </summary>
    public T[] GetArray<T>(string sheetName, int index, string key, T[] defaultValue = null)
    {
        try
        {
            var value = GetValue(sheetName, index, key);
            if (value == null) return defaultValue ?? Array.Empty<T>();
            
            if (value is T[] typedArray) return typedArray;
            if (value is Array arr)
            {
                T[] result = new T[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    result[i] = (T)Convert.ChangeType(arr.GetValue(i), typeof(T));
                }
                return result;
            }
            
            return defaultValue ?? Array.Empty<T>();
        }
        catch
        {
            Debug.LogWarning($"⚠️ {sheetName}시트의 {index}행 {key} 배열 변환 실패. 기본값 반환");
            return defaultValue ?? Array.Empty<T>();
        }
    }

    /// <summary>
    /// 시트의 특정 행이 존재하는지 확인
    /// </summary>
    public bool HasRow(string sheetName, int index)
    {
        if (!sheetData.ContainsKey(sheetName)) return false;
        return index >= 0 && index < sheetData[sheetName].Count;
    }
    
    #endregion
}