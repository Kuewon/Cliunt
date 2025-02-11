using UnityEngine;
using System.Collections.Generic;
using System;

public class WaveMovementController : MonoBehaviour
{
    public static WaveMovementController Instance { get; private set; }
    
    private HashSet<EnemyMoveController> currentWaveEnemies = new HashSet<EnemyMoveController>();
    private bool isWaveMovementEnabled = false;

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
        if (BackgroundScroller.Instance != null)
        {
            BackgroundScroller.Instance.OnScrollComplete += EnableWaveMovement;
        }
    }

    private void OnDestroy()
    {
        if (BackgroundScroller.Instance != null)
        {
            BackgroundScroller.Instance.OnScrollComplete -= EnableWaveMovement;
        }
    }

    public void RegisterEnemy(EnemyMoveController enemy)
    {
        if (enemy != null)
        {
            currentWaveEnemies.Add(enemy);
            enemy.SetMovementEnabled(isWaveMovementEnabled);
        }
    }

    public void UnregisterEnemy(EnemyMoveController enemy)
    {
        if (enemy != null)
        {
            currentWaveEnemies.Remove(enemy);
        }
    }

    private void EnableWaveMovement()
    {
        isWaveMovementEnabled = true;
        foreach (var enemy in currentWaveEnemies)
        {
            if (enemy != null)
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