using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    
    [Header("Height Range")]
    [SerializeField] private float minSpawnHeight = 0f;    // 최소 스폰 높이
    [SerializeField] private float maxSpawnHeight = 2f;    // 최대 스폰 높이
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnRange = true;   // 기즈모로 스폰 범위 표시

    private float timer = 0f;
    private Camera mainCamera;
    private float spawnX;

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateSpawnPosition();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private void CalculateSpawnPosition()
    {
        if (mainCamera != null)
        {
            // 화면 오른쪽 바깥에서 스폰
            spawnX = mainCamera.ViewportToWorldPoint(new Vector3(1.1f, 0f, 0f)).x;
        }
        else
        {
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다!");
            spawnX = 10f; // 기본값
        }
    }

    private void SpawnEnemy()
    {
        // minSpawnHeight와 maxSpawnHeight 사이의 랜덤한 높이 생성
        float randomHeight = Random.Range(minSpawnHeight, maxSpawnHeight);
        Vector2 spawnPosition = new Vector2(spawnX, randomHeight);
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnRange) return;

        // 스폰 범위를 시각적으로 표시
        Gizmos.color = Color.yellow;
        Vector3 lineStart = new Vector3(spawnX, minSpawnHeight, 0);
        Vector3 lineEnd = new Vector3(spawnX, maxSpawnHeight, 0);
        Gizmos.DrawLine(lineStart, lineEnd);

        // 범위의 시작과 끝을 표시
        float sphereRadius = 0.2f;
        Gizmos.DrawWireSphere(lineStart, sphereRadius);
        Gizmos.DrawWireSphere(lineEnd, sphereRadius);
    }
}