using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Waves/Wave Data")]
public class WaveData : ScriptableObject
{
    public float delayBeforeWave = 0.5f;
    public EnemySpawnEntry[] enemies;
}

[Serializable]
public class EnemySpawnEntry
{
    public EnemyData enemyData;
    public int count = 1;
    public float delayBetweenSpawns = 0.25f;
}
