using UnityEngine;
using System.Collections.Generic;

public class WaveMovementController : MonoBehaviour
{
    public static WaveMovementController Instance { get; private set; }
    
    private HashSet<EnemyMoveController> currentWaveEnemies = new HashSet<EnemyMoveController>();
    private bool isWaveMovementEnabled = false;
    private RectTransform topIngameRect;

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

        // TopIngame 찾기
        GameObject topIngame = GameObject.FindWithTag("TopIngame");
        if (topIngame != null)
        {
            topIngameRect = topIngame.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (ParallaxBackgroundScroller.Instance != null)
        {
            ParallaxBackgroundScroller.Instance.OnScrollComplete += HandleScrollComplete;
            ParallaxBackgroundScroller.Instance.OnScrollUpdate += UpdateEnemyPositions;
        }
    }

    private void UpdateEnemyPositions(float scrollProgress)
    {
        if (topIngameRect == null) return;

        float totalMovement = topIngameRect.rect.width;

        foreach (var enemy in currentWaveEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                var rectTransform = enemy.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 startPos = enemy.InitialPosition;
                    Vector2 targetPos = startPos - new Vector2(totalMovement, 0);
                    rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, scrollProgress);
                }
            }
        }
    }

    private void HandleScrollComplete()
    {
        EnableWaveMovement();
    }

    private void OnDestroy()
    {
        if (ParallaxBackgroundScroller.Instance != null)
        {
            ParallaxBackgroundScroller.Instance.OnScrollComplete -= HandleScrollComplete;
            ParallaxBackgroundScroller.Instance.OnScrollUpdate -= UpdateEnemyPositions;
        }
    }

    public void RegisterEnemy(EnemyMoveController enemy)
    {
        if (enemy != null)
        {
            currentWaveEnemies.Add(enemy);
            
            // 초기 위치 저장
            var rectTransform = enemy.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                enemy.InitialPosition = rectTransform.anchoredPosition;
            }
            
            enemy.SetMovementEnabled(isWaveMovementEnabled);
        }
    }

    public void UnregisterEnemy(EnemyMoveController enemy)
    {
        currentWaveEnemies.Remove(enemy);
    }

    private void EnableWaveMovement()
    {
        isWaveMovementEnabled = true;
        foreach (var enemy in currentWaveEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.SetMovementEnabled(true);
            }
        }
    }

    public void ResetWaveMovement()
    {
        isWaveMovementEnabled = false;
        currentWaveEnemies.Clear();
    }
}