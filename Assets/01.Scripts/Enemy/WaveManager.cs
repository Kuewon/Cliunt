using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Stage Settings")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int maxStage = 10;

    [Header("Wave Settings")]
    [SerializeField] private float nextWaveDelay = 2f;
    [SerializeField] private float nextStageDelay = 3f;

    private List<List<int>> waveEnemyIndices = new List<List<int>>();
    private float enemyAttackMultiplier;
    private float enemyHealthMultiplier;
    private float enemyAttackSpeedMultiplier;
    private int currentWaveIndex = -1;
    private HashSet<EnemyHealth> activeEnemies = new HashSet<EnemyHealth>();
    private bool isWaveInProgress = false;
    private bool isStageTransitioning = false;
    private bool skipStageTransition = false;
    public bool IsStageTransitioning => isStageTransitioning;

    [SerializeField] private float _enemyDropGoldMultiplier = 1.0f;
    public float enemyDropGoldMultiplier => _enemyDropGoldMultiplier;

    public event Action<int> OnStageChanged;
    public event Action OnAllStagesCompleted;
    public event Action OnWaveCompleted;
    public event Action<int> OnNewWaveStarted;

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
        skipStageTransition = true;
        LoadWaveData();
        currentWaveIndex = -1;
        StartNextWave();
    }

    public void StartStage(int stageNumber)
    {
        if (isStageTransitioning)
        {
            Debug.Log("스테이지 전환 중에는 새로운 스테이지를 시작할 수 없습니다.");
            return;
        }

        currentStage = stageNumber;
        currentWaveIndex = -1;
        waveEnemyIndices.Clear();
        isWaveInProgress = false;

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();

        LoadWaveData();

        if (waveEnemyIndices.Count == 0)
        {
            Debug.LogError($"Stage {stageNumber}의 웨이브 데이터가 비어있습니다!");
            return;
        }

        OnStageChanged?.Invoke(stageNumber);
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

        try
        {
            // 멀티플라이어 데이터 로드
            enemyAttackMultiplier = Convert.ToSingle(waveData["enemyAttackMultiplier"]);
            enemyHealthMultiplier = Convert.ToSingle(waveData["enemyHealthMultiplier"]);
            enemyAttackSpeedMultiplier = Convert.ToSingle(waveData["enemyAttackSpeedMultiplier"]);
            _enemyDropGoldMultiplier = Convert.ToSingle(waveData["enemyGoldMultiplier"]);

            // 2차원 배열로 waves 데이터 가져오기
            int[][] wavesArray = GameData.Instance.GetInt2DArray("WaveInfo", currentStage - 1, "waves");
            waveEnemyIndices.Clear();

            // 각 웨이브 데이터를 List<int>로 변환하여 추가
            foreach (int[] enemyGroup in wavesArray)
            {
                waveEnemyIndices.Add(new List<int>(enemyGroup));
            }

            Debug.Log($"Stage {currentStage} 웨이브 데이터 로드 완료: 총 {waveEnemyIndices.Count}개 웨이브");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading wave data: {e.Message}\n{e.StackTrace}");
        }
    }

    public bool StartNextWave()
    {
        if (isWaveInProgress)
        {
            return false;
        }

        currentWaveIndex++;
        if (currentWaveIndex >= waveEnemyIndices.Count)
        {
            return false;
        }

        SpawnWaveEnemy();
        OnNewWaveStarted?.Invoke(currentWaveIndex + 1);
        isWaveInProgress = true;
        return true;
    }

    private void SpawnWaveEnemy()
    {
        if (currentWaveIndex < 0 || currentWaveIndex >= waveEnemyIndices.Count)
        {
            Debug.LogError("Invalid wave index!");
            return;
        }

        var currentWave = waveEnemyIndices[currentWaveIndex];
        StartCoroutine(SpawnWaveEnemiesSequentially(currentWave));
    }

    private IEnumerator SpawnWaveEnemiesSequentially(List<int> enemyIndices)
    {
        foreach (int enemyIndex in enemyIndices)
        {
            SpawnEnemy(enemyIndex);
        }
        yield return null;
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

    public void RegisterEnemy(EnemyHealth enemy)
    {
        activeEnemies.Add(enemy);
        enemy.OnEnemyDeath += () => RemoveEnemy(enemy);
    }

    private void RemoveEnemy(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);
    
        // 웨이브의 적이 모두 처치되었는지 확인
        if (activeEnemies.Count == 0)
        {
            isWaveInProgress = false;
            OnWaveCompleted?.Invoke();

            // 전환 중이 아닐 때만 다음 웨이브/스테이지 처리
            if (!isStageTransitioning)
            {
                if (HasNextWave())
                {
                    StartCoroutine(StartNextWaveWithDelay());
                }
                else
                {
                    StartCoroutine(StartNextStageWithDelay());
                }
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
        if (isStageTransitioning)
        {
            Debug.Log("이미 스테이지 전환 중입니다.");
            yield break;
        }

        isStageTransitioning = true;

        if (currentStage < maxStage)
        {
            yield return new WaitForSeconds(nextStageDelay);

            currentStage++;
            skipStageTransition = false;

            // 스테이지 변경 이벤트 발생
            OnStageChanged?.Invoke(currentStage);

            // 웨이브 데이터 로드
            LoadWaveData();

            while (BackgroundScroller.Instance != null && BackgroundScroller.Instance.IsScrolling)
            {
                yield return null;
            }

            // 다음 웨이브 시작 전에 잠시 대기
            yield return new WaitForSeconds(0.5f);

            // 새로운 웨이브 시작 준비
            currentWaveIndex = -1;
            isWaveInProgress = false;

            // 다음 웨이브 시작
            StartNextWave();

            // 전환 완료
            isStageTransitioning = false;
        }
        else
        {
            isStageTransitioning = false;
            Debug.Log("모든 스테이지를 완료했습니다!");
            OnAllStagesCompleted?.Invoke();
        }
    }

    public bool ShouldPlayStageTransition()
    {
        if (skipStageTransition)
        {
            skipStageTransition = false;
            return false;
        }
        return true;
    }

    // 상태 확인용 메서드들
    public int GetCurrentStage() => currentStage;
    public int GetCurrentWave() => currentWaveIndex + 1;
    public int GetTotalWaves() => waveEnemyIndices.Count;
    public int GetRemainingEnemies() => activeEnemies.Count;
    public bool IsCurrentWaveComplete() => !isWaveInProgress && activeEnemies.Count == 0;
    public bool HasNextWave() => currentWaveIndex < waveEnemyIndices.Count - 1;
    public bool IsWaveInProgress() => isWaveInProgress;
}