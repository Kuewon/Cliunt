using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private int currentStage = 1;
    [SerializeField] private float spawnInterval = 5f;

    private List<int[]> waveEnemyIndices = new List<int[]>();
    private float enemyAttackMultiplier;
    private float enemyHealthMultiplier;
    private float enemyAttackSpeedMultiplier;
    private int currentWaveIndex = -1;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadWaveData();
        StartNextWave();
    }

    private void LoadWaveData()
    {
        var waveData = GameData.Instance.GetRow("WaveInfo", currentStage - 1);
        if (waveData == null) return;

        enemyAttackMultiplier = Convert.ToSingle(waveData["enemyAttackMultiplier"]);
        enemyHealthMultiplier = Convert.ToSingle(waveData["enemyHealthMultiplier"]);
        enemyAttackSpeedMultiplier = Convert.ToSingle(waveData["enemyAttackSpeedMultiplier"]);

        var wavesValue = waveData["waves"];

        // waves가 이미 int[]로 파싱되어 있는 경우
        if (wavesValue is int[] intArray)
        {
            waveEnemyIndices.Add(intArray);
            Debug.Log($"Added wave with {intArray.Length} enemies");
        }
        else
        {
            Debug.LogError($"Unexpected waves data type: {wavesValue?.GetType()}");
        }
    }

    public bool StartNextWave()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        currentWaveIndex++;
        if (currentWaveIndex >= waveEnemyIndices.Count)
        {
            Debug.Log("모든 웨이브 완료!");
            return false;
        }

        spawnCoroutine = StartCoroutine(SpawnWaveEnemiesSequentially());
        return true;
    }

    private IEnumerator SpawnWaveEnemiesSequentially()
    {
        int[] enemyIndices = waveEnemyIndices[currentWaveIndex];

        foreach (int enemyIndex in enemyIndices)
        {
            SpawnEnemy(enemyIndex);
            yield return new WaitForSeconds(spawnInterval);
        }

        spawnCoroutine = null;
    }

    private void SpawnEnemy(int enemyIndex)
    {
        var enemyStats = GameData.Instance.GetRow("EnemyStats", enemyIndex);
        if (enemyStats == null) return;

        EnemySpawnController.Instance.SpawnEnemyWithStats(
            enemyStats,
            enemyHealthMultiplier,
            enemyAttackMultiplier,
            enemyAttackSpeedMultiplier
        );

        Debug.Log($"Spawned enemy with index: {enemyIndex}");
    }

    // 현재 웨이브의 스폰이 완료되었는지 확인
    public bool IsCurrentWaveComplete()
    {
        return spawnCoroutine == null;
    }

    // 다음 웨이브가 있는지 확인
    public bool HasNextWave()
    {
        return currentWaveIndex < waveEnemyIndices.Count - 1;
    }
}