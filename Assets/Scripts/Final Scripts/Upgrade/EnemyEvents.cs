using System;
using UnityEngine;

public static class EnemyEvents
{
    public static event Action<EnemyBase> OnEnemyDied;

    public static void RaiseEnemyDied(EnemyBase enemy)
    {
        OnEnemyDied?.Invoke(enemy);
    }
}
