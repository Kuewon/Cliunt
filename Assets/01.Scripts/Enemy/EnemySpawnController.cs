using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemySpawnController : MonoBehaviour
{
    public static EnemySpawnController Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab; 
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
            // 스테이지 전환 시작 시
            WaveManager.Instance.OnStageChanged += HandleStageChange;
            
            // 새로운 웨이브 시작 시 (스테이지 전환 완료 후)
            WaveManager.Instance.OnNewWaveStarted += (wave) => {
                isStageTransitioning = false;
            };
        }
    }

    private void ValidateComponents()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab이 할당되지 않았습니다!");
        }

        // TopIngame 태그로 오브젝트 찾기
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

        // 플레이어 찾기
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
        float spawnX;
        
        if (isStageTransitioning)
        {
            // 스테이지 전환 중일 때는 현재 width의 80% 지점에서 스폰
            spawnX = width * 0.8f;
        }
        else
        {
            // 첫 스테이지나 일반 웨이브에서는 현재 스테이지 끝 + 80% 지점에서 스폰
            spawnX = width + (width * 0.8f);
        }
    
        // 화면 중앙 기준으로 보정
        spawnX = spawnX - (width / 2);
        float spawnY = playerRect.anchoredPosition.y;

        return new Vector2(spawnX, spawnY);
    }


    public void SpawnEnemyWithStats(
    Dictionary<string, object> enemyStats,
    float healthMultiplier,
    float attackMultiplier,
    float attackSpeedMultiplier)
    {
        if (enemyPrefab == null || topIngameRect == null)
        {
            Debug.LogError("필요한 컴포넌트가 없습니다!");
            return;
        }

        try
        {
            Vector2 spawnPosition = GetSpawnPosition();
            
            float randomYOffset = UnityEngine.Random.Range(-50f, 50f);
            spawnPosition.y += randomYOffset;

            GameObject enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, topIngameRect);
            
            RectTransform enemyRect = enemy.GetComponent<RectTransform>();
            if (enemyRect != null)
            {
                enemyRect.anchoredPosition = spawnPosition;
                // 스폰될 때 왼쪽을 보도록 설정
                Vector3 scale = enemyRect.localScale;
                scale.x = -Mathf.Abs(scale.x);  // x 스케일을 음수로 만들어 왼쪽을 보도록
                enemyRect.localScale = scale;
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

        // 월드 스페이스 위치로 변환
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
        // 첫 스테이지(stage 1)가 아닐 때만 전환 모드 활성화
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