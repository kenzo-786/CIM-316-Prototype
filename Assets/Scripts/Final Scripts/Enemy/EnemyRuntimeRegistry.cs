using System;
using UnityEngine;

public static class EnemyRuntimeRegistry
{
    public static event Action<EnemyBase> OnEnemySpawnedRuntime;

    public static void RaiseEnemySpawned(EnemyBase enemy)
    {
        if (enemy != null)
            OnEnemySpawnedRuntime?.Invoke(enemy);
    }
}
