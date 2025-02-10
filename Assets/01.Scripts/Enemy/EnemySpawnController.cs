using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemySpawnController : MonoBehaviour
{
    public static EnemySpawnController Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Height Range")]
    [SerializeField] private float minSpawnHeightPercentage = 0f;
    [SerializeField] private float maxSpawnHeightPercentage = 0.7f;
    [SerializeField] private float spawnXOffsetPercentage = 1.1f;

    [Header("Debug")]
    [SerializeField] private bool showSpawnRange = true;

    [SerializeField] private float _enemyDropGold = 0;
    public float enemyDropGold => _enemyDropGold;

    private RectTransform canvasRect;
    private Camera mainCamera;

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
        GameObject topIngame = GameObject.FindGameObjectWithTag("TopIngame");
        if (topIngame == null)
        {
            Debug.LogError("TopIngame 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        canvasRect = topIngame.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("TopIngame 오브젝트에 RectTransform 컴포넌트가 없습니다!");
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다!");
            return;
        }
    }

    private Vector2 GetSpawnPosition()
    {
        if (canvasRect == null || mainCamera == null) return Vector2.zero;

        // 캔버스의 크기와 위치 정보 가져오기
        Rect canvasRectSize = canvasRect.rect;
        Vector2 canvasCenter = canvasRect.position;
        
        // 스폰 X 위치 계산 (캔버스 오른쪽 끝 기준)
        float rightEdgeX = canvasCenter.x + (canvasRectSize.width * 0.5f);
        float spawnX = rightEdgeX + (canvasRectSize.width * (spawnXOffsetPercentage - 1f));

        // 높이 범위 계산
        float minY = canvasCenter.y + (canvasRectSize.height * (-0.5f + minSpawnHeightPercentage));
        float maxY = canvasCenter.y + (canvasRectSize.height * (-0.5f + maxSpawnHeightPercentage));
        float randomY = UnityEngine.Random.Range(minY, maxY);

        Debug.Log($"Spawn Position - X: {spawnX}, Y: {randomY}");
        return new Vector2(spawnX, randomY);
    }

    public void SpawnEnemyWithStats(
        Dictionary<string, object> enemyStats,
        float healthMultiplier,
        float attackMultiplier,
        float attackSpeedMultiplier)
    {
        if (canvasRect == null) return;

        // 스폰 위치 계산
        Vector2 spawnPosition = GetSpawnPosition();

        // 적 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.SetParent(canvasRect, false);

        // 체력 설정
        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            float baseHealth = Convert.ToSingle(enemyStats["baseHealth"]);
            health.SetMaxHealth(baseHealth * healthMultiplier);
        }

        // 이동 및 공격 설정
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
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnRange || canvasRect == null) return;

        // 캔버스의 크기와 위치 정보 가져오기
        Rect canvasRectSize = canvasRect.rect;
        Vector2 center = canvasRect.position;

        float rightEdgeX = center.x + (canvasRectSize.width * 0.5f);
        float spawnX = rightEdgeX + (canvasRectSize.width * (spawnXOffsetPercentage - 1f));

        float minY = center.y + (canvasRectSize.height * (-0.5f + minSpawnHeightPercentage));
        float maxY = center.y + (canvasRectSize.height * (-0.5f + maxSpawnHeightPercentage));

        Gizmos.color = Color.yellow;
        Vector3 lineStart = new Vector3(spawnX, minY, 0);
        Vector3 lineEnd = new Vector3(spawnX, maxY, 0);
        Gizmos.DrawLine(lineStart, lineEnd);

        float sphereRadius = 20f;
        Gizmos.DrawWireSphere(lineStart, sphereRadius);
        Gizmos.DrawWireSphere(lineEnd, sphereRadius);
    }
}