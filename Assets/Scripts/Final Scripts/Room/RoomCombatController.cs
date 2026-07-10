using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomCombatController : MonoBehaviour
{
    [SerializeField] private ObjectPool xpGemPool;
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private RoomDifficultySettings difficultySettings;

    private readonly List<EnemyBase> aliveEnemies = new List<EnemyBase>();
    private readonly List<ExperienceGem> droppedGems = new List<ExperienceGem>();

    private RoomData currentRoomData;
    private RoomLayout currentLayout;
    private Transform player;

    private RoomDifficultySnapshot currentDifficulty;
    private bool combatActive;

    public event Action OnRoomCombatCleared;

    private void OnEnable()
    {
        EnemyRuntimeRegistry.OnEnemySpawnedRuntime += RegisterRuntimeEnemy;
    }

    private void OnDisable()
    {
        EnemyRuntimeRegistry.OnEnemySpawnedRuntime -= RegisterRuntimeEnemy;
    }

    public void StartRoomCombat(RoomData roomData, RoomLayout layout, Transform playerTransform, int roomIndex)
    {
        StopAllCoroutines();

        currentRoomData = roomData;
        currentLayout = layout;
        player = playerTransform;

        currentDifficulty = difficultySettings != null
            ? difficultySettings.GetDifficulty(roomIndex)
            : new RoomDifficultySnapshot
            {
                healthMultiplier = 1f,
                damageMultiplier = 1f,
                moveSpeedMultiplier = 1f
            };

        combatActive = true;

        aliveEnemies.Clear();
        droppedGems.Clear();

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        if (currentRoomData.waves == null || currentRoomData.waves.Length == 0)
        {
            FinishRoomCombat();
            yield break;
        }

        foreach (WaveData wave in currentRoomData.waves)
        {
            yield return new WaitForSeconds(wave.delayBeforeWave);

            foreach (EnemySpawnEntry entry in wave.enemies)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData);
                    yield return new WaitForSeconds(entry.delayBetweenSpawns);
                }
            }

            yield return new WaitUntil(() => aliveEnemies.Count == 0);
        }

        FinishRoomCombat();
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        if (enemyData == null || enemyData.prefab == null)
            return;

        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPosition = spawnPoint != null
            ? spawnPoint.position
            : currentLayout.transform.position;

        GameObject enemyObject = Instantiate(enemyData.prefab, spawnPosition, Quaternion.identity);
        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

        if (enemy == null)
            return;

        enemy.Initialize(enemyData, player);
        enemy.ApplyDifficulty(currentDifficulty);

        enemy.OnEnemyDied += HandleEnemyDied;
        aliveEnemies.Add(enemy);
    }

    private void RegisterRuntimeEnemy(EnemyBase enemy)
    {
        if (!combatActive || enemy == null)
            return;

        if (aliveEnemies.Contains(enemy))
            return;

        enemy.OnEnemyDied += HandleEnemyDied;
        aliveEnemies.Add(enemy);
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        enemy.OnEnemyDied -= HandleEnemyDied;
        aliveEnemies.Remove(enemy);
        DropXp(enemy);
    }

    private void DropXp(EnemyBase enemy)
    {
        if (xpGemPool == null || enemy.EnemyData == null)
            return;

        GameObject gemObject = xpGemPool.Get(enemy.transform.position, Quaternion.identity);
        ExperienceGem gem = gemObject.GetComponent<ExperienceGem>();

        if (gem == null)
            return;

        gem.Initialize(enemy.EnemyData.xpDropAmount);
        droppedGems.Add(gem);
    }

    private void FinishRoomCombat()
    {
        combatActive = false;
        MagnetizeAllGems();
        OnRoomCombatCleared?.Invoke();
    }

    private void MagnetizeAllGems()
    {
        foreach (ExperienceGem gem in droppedGems)
        {
            if (gem != null)
                gem.MagnetizeTo(player, playerExperience);
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        Transform root = currentLayout.EnemySpawnRoot;

        if (root == null || root.childCount == 0)
            return null;

        return root.GetChild(UnityEngine.Random.Range(0, root.childCount));
    }
}
