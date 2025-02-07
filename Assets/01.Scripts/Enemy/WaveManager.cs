using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Stage Settings")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int maxStage = 10; // 최대 스테이지 수

    [Header("Wave Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float nextWaveDelay = 2f;
    [SerializeField] private float nextStageDelay = 3f; // 다음 스테이지로 넘어가기 전 대기 시간

    private List<int[]> waveEnemyIndices = new List<int[]>();
    private float enemyAttackMultiplier;
    private float enemyHealthMultiplier;
    private float enemyAttackSpeedMultiplier;
    private int currentWaveIndex = -1;
    private Coroutine spawnCoroutine;
    private HashSet<EnemyHealth> activeEnemies = new HashSet<EnemyHealth>();

    // 스테이지 변경 이벤트
    public event Action<int> OnStageChanged;
    public event Action OnAllStagesCompleted;

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
        StartStage(currentStage);
    }

    public void StartStage(int stageNumber)
    {
        currentStage = stageNumber;
        currentWaveIndex = -1;
        waveEnemyIndices.Clear();
        
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();

        LoadWaveData();
        OnStageChanged?.Invoke(currentStage);
        StartNextWave();
    }

    private void LoadWaveData()
    {
        var waveData = GameData.Instance.GetRow("WaveInfo", currentStage - 1);
        if (waveData == null)
        {
            Debug.LogError($"Stage {currentStage}의 데이터를 찾을 수 없습니다!");
            return;
        }

        enemyAttackMultiplier = Convert.ToSingle(waveData["enemyAttackMultiplier"]);
        enemyHealthMultiplier = Convert.ToSingle(waveData["enemyHealthMultiplier"]);
        enemyAttackSpeedMultiplier = Convert.ToSingle(waveData["enemyAttackSpeedMultiplier"]);

        var wavesValue = waveData["waves"];
        
        if (wavesValue is int[] intArray)
        {
            waveEnemyIndices.Add(intArray);
        }
        else
        {
            Debug.LogError($"Unexpected waves data type: {wavesValue?.GetType()}");
        }
    }

    public void RegisterEnemy(EnemyHealth enemy)
    {
        activeEnemies.Add(enemy);
        enemy.OnEnemyDeath += () => RemoveEnemy(enemy);
    }

    private void RemoveEnemy(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);
        
        // 현재 웨이브의 스폰이 완료되고 모든 적이 죽었다면
        if (spawnCoroutine == null && activeEnemies.Count == 0)
        {
            // 다음 웨이브가 있다면 다음 웨이브 시작
            if (HasNextWave())
            {
                StartCoroutine(StartNextWaveWithDelay());
            }
            // 다음 웨이브가 없다면 다음 스테이지로
            else
            {
                StartCoroutine(StartNextStageWithDelay());
            }
        }
    }

    private IEnumerator StartNextWaveWithDelay()
    {
        yield return new WaitForSeconds(nextWaveDelay);
        StartNextWave();
    }

    private IEnumerator StartNextStageWithDelay()
    {
        yield return new WaitForSeconds(nextStageDelay);
        
        if (currentStage < maxStage)
        {
            StartStage(currentStage + 1);
        }
        else
        {
            Debug.Log("모든 스테이지를 완료했습니다!");
            OnAllStagesCompleted?.Invoke();
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
    }

    public int GetCurrentStage() => currentStage;
    public int GetCurrentWave() => currentWaveIndex + 1;
    public int GetRemainingEnemies() => activeEnemies.Count;
    public bool IsCurrentWaveComplete() => spawnCoroutine == null && activeEnemies.Count == 0;
    public bool HasNextWave() => currentWaveIndex < waveEnemyIndices.Count - 1;
}