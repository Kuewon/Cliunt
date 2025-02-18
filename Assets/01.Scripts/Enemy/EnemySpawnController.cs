using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemySpawnController : MonoBehaviour
{
    public static EnemySpawnController Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs; // 0: 근거리, 1: 원거리, 2: 보스
    [SerializeField] private RuntimeAnimatorController[] enemyAnimators; // 각 적 타입별 애니메이터

    [Header("Spawn Settings")]
    private float spawnXOffsetPercentage = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool showSpawnRange = true;

    private RectTransform topIngameRect;
    private RectTransform playerRect;
    private bool isStageTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ValidateComponents();
    }

    private void Start()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnStageChanged += HandleStageChange;
            WaveManager.Instance.OnNewWaveStarted += (wave) => {
                isStageTransitioning = false;
            };
        }
    }

    private void ValidateComponents()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length != 3)
        {
            Debug.LogError("Enemy Prefabs가 올바르게 할당되지 않았습니다! (근거리, 원거리, 보스 3개 필요)");
            return;
        }

        // 각 프리팹의 활성화 상태 확인
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i] == null)
            {
                Debug.LogError($"Enemy Prefab {i}가 할당되지 않았습니다!");
                continue;
            }

            if (!enemyPrefabs[i].activeSelf)
            {
                Debug.LogWarning($"Enemy Prefab {i}가 비활성화되어 있습니다. 프리팹을 활성화해주세요!");
            }
        }

        if (enemyAnimators == null || enemyAnimators.Length != 3)
        {
            Debug.LogError("Enemy Animators가 올바르게 할당되지 않았습니다! (근거리, 원거리, 보스 3개 필요)");
        }

        GameObject topIngame = GameObject.FindWithTag("TopIngame");
        if (topIngame == null)
        {
            Debug.LogError("TopIngame 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        topIngameRect = topIngame.GetComponent<RectTransform>();
        if (topIngameRect == null)
        {
            Debug.LogError("TopIngame 오브젝트에 RectTransform이 없습니다!");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        playerRect = player.GetComponent<RectTransform>();
        if (playerRect == null)
        {
            Debug.LogError("Player 오브젝트에 RectTransform이 없습니다!");
            return;
        }
    }

    private Vector2 GetSpawnPosition()
    {
        if (topIngameRect == null || playerRect == null)
        {
            ValidateComponents();
            if (topIngameRect == null || playerRect == null)
            {
                Debug.LogError("필요한 컴포넌트가 없습니다!");
                return Vector2.zero;
            }
        }

        float width = topIngameRect.rect.width;
        float baseSpawnX = width * 0.8f;
        float randomXOffset = UnityEngine.Random.Range(-100f, 100f);
        float spawnX = (baseSpawnX + randomXOffset) - (width / 2);
        float randomYOffset = UnityEngine.Random.Range(-50f, 50f);
        float spawnY = playerRect.anchoredPosition.y + randomYOffset;

        return new Vector2(spawnX, spawnY);
    }

    public void SpawnEnemyWithStats(
        Dictionary<string, object> enemyStats,
        float healthMultiplier,
        float attackMultiplier,
        float attackSpeedMultiplier)
    {
        if (enemyPrefabs == null || topIngameRect == null)
        {
            Debug.LogError("필요한 컴포넌트가 없습니다!");
            return;
        }

        try
        {
            // enemyStats에서 enemyType 가져오기 (0: 근거리, 1: 원거리, 2: 보스)
            int enemyType;
            if (!enemyStats.ContainsKey("enemyType"))
            {
                Debug.LogError("enemyStats에 enemyType이 없습니다!");
                return;
            }

            // enemyType이 int나 float 형태로 들어올 수 있으므로 안전하게 변환
            if (enemyStats["enemyType"] is int intType)
            {
                enemyType = intType;
            }
            else if (enemyStats["enemyType"] is float floatType)
            {
                enemyType = (int)floatType;
            }
            else if (enemyStats["enemyType"] is double doubleType)
            {
                enemyType = (int)doubleType;
            }
            else
            {
                Debug.LogError($"유효하지 않은 enemyType 형식: {enemyStats["enemyType"]?.GetType()}");
                return;
            }

            if (enemyType < 0 || enemyType >= enemyPrefabs.Length)
            {
                Debug.LogError($"유효하지 않은 enemy type: {enemyType}");
                return;
            }

            Vector2 spawnPosition = GetSpawnPosition();
            float randomYOffset = UnityEngine.Random.Range(-50f, 50f);
            spawnPosition.y += randomYOffset;

            // 해당 타입의 프리팹으로 적 생성
            GameObject enemy = Instantiate(enemyPrefabs[enemyType], Vector3.zero, Quaternion.identity, topIngameRect);
            enemy.SetActive(true); // 생성된 적 활성화

            // 애니메이터 설정
            Animator animator = enemy.GetComponent<Animator>();
            if (animator != null && enemyAnimators[enemyType] != null)
            {
                animator.runtimeAnimatorController = enemyAnimators[enemyType];
            }

            RectTransform enemyRect = enemy.GetComponent<RectTransform>();
            if (enemyRect != null)
            {
                enemyRect.anchoredPosition = spawnPosition;
            }

            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                float baseHealth = Convert.ToSingle(enemyStats["baseHealth"]);
                health.SetMaxHealth(baseHealth * healthMultiplier);
            }

            var moveController = enemy.GetComponent<EnemyMoveController>();
            if (moveController != null)
            {
                float baseAttackDamage = Convert.ToSingle(enemyStats["baseAttackDamage"]);
                float baseAttackSpeed = Convert.ToSingle(enemyStats["baseAttackSpeed"]);
                float movementSpeed = Convert.ToSingle(enemyStats["movementSpeed"]);
                float attackRange = Convert.ToSingle(enemyStats["attackRange"]);
                float gold = Convert.ToSingle(enemyStats["enemyDropGold"]);

                moveController.SetStats(
                    baseAttackDamage * attackMultiplier,
                    baseAttackSpeed * attackSpeedMultiplier,
                    movementSpeed,
                    attackRange,
                    gold
                );
            }

            if (health != null && WaveManager.Instance != null)
            {
                WaveManager.Instance.RegisterEnemy(health);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error spawning enemy: {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnRange || topIngameRect == null || playerRect == null) return;

        Vector3 spawnPos = topIngameRect.TransformPoint(
            new Vector3(topIngameRect.sizeDelta.x * spawnXOffsetPercentage,
                playerRect.anchoredPosition.y,
                0)
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPos, 20f);
    }

    private void HandleStageChange(int newStage)
    {
        isStageTransitioning = newStage > 1;
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnStageChanged -= HandleStageChange;
            WaveManager.Instance.OnNewWaveStarted -= (wave) => { isStageTransitioning = false; };
        }
    }
}