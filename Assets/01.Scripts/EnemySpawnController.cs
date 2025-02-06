using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;  // 적 프리팹
    [SerializeField] private float spawnInterval = 3f;  // 스폰 간격
    [SerializeField] private float spawnHeight = 1f;   // y축 1로 설정

    private float timer = 0f;
    private Camera mainCamera;
    private float spawnX;  // 스폰 X 좌표

    private void Start()
    {
        mainCamera = Camera.main;
        // 화면 오른쪽 바깥에서 스폰하도록 위치 계산
        spawnX = mainCamera.ViewportToWorldPoint(new Vector3(1.1f, 0f, 0f)).x;
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

    private void SpawnEnemy()
    {
        Vector2 spawnPosition = new Vector2(spawnX, 1f); // y축을 1로 고정
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}