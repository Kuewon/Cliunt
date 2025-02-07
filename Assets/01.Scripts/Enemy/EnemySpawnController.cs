using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemySpawnController : MonoBehaviour
{
    public static EnemySpawnController Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Height Range")]
    [SerializeField] private float minSpawnHeight = 0f;
    [SerializeField] private float maxSpawnHeight = 2f;

    [Header("Debug")]
    [SerializeField] private bool showSpawnRange = true;

    private Camera mainCamera;
    private float spawnX;

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
        mainCamera = Camera.main;
        CalculateSpawnPosition();
    }

    private void CalculateSpawnPosition()
    {
        if (mainCamera != null)
        {
            spawnX = mainCamera.ViewportToWorldPoint(new Vector3(1.1f, 0f, 0f)).x;
        }
        else
        {
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다!");
            spawnX = 10f;
        }
    }

    public void SpawnEnemyWithStats(
        Dictionary<string, object> enemyStats,
        float healthMultiplier,
        float attackMultiplier,
        float attackSpeedMultiplier)
    {
        float randomHeight = UnityEngine.Random.Range(minSpawnHeight, maxSpawnHeight);
        Vector2 spawnPosition = new Vector2(spawnX, randomHeight);

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

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

            moveController.SetStats(
                baseAttackDamage * attackMultiplier,
                baseAttackSpeed * attackSpeedMultiplier,
                movementSpeed,
                attackRange
            );
        }
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnRange) return;

        Gizmos.color = Color.yellow;
        Vector3 lineStart = new Vector3(spawnX, minSpawnHeight, 0);
        Vector3 lineEnd = new Vector3(spawnX, maxSpawnHeight, 0);
        Gizmos.DrawLine(lineStart, lineEnd);

        float sphereRadius = 0.2f;
        Gizmos.DrawWireSphere(lineStart, sphereRadius);
        Gizmos.DrawWireSphere(lineEnd, sphereRadius);
    }
}