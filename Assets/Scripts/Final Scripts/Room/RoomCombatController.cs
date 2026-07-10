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
    private RoomDifficultySnapshot currentDifficulty = RoomDifficultySnapshot.Default;
    private Coroutine waveRoutine;
    private bool combatRunning;

    public event Action OnRoomCombatCleared;

    private void OnEnable()
    {
        EnemyRuntimeRegistry.OnEnemySpawnedRuntime += TrackRuntimeEnemy;
    }

    private void OnDisable()
    {
        EnemyRuntimeRegistry.OnEnemySpawnedRuntime -= TrackRuntimeEnemy;
        StopAllCoroutines();
        ClearEnemySubscriptions();
    }

    public void StartRoomCombat(RoomData roomData, RoomLayout layout, Transform playerTransform)
    {
        StartRoomCombat(roomData, layout, playerTransform, 0);
    }

    public void StartRoomCombat(RoomData roomData, RoomLayout layout, Transform playerTransform, int roomIndex)
    {
        StopCurrentCombat();

        currentRoomData = roomData;
        currentLayout = layout;
        player = playerTransform;
        combatRunning = true;

        currentDifficulty = difficultySettings != null
            ? difficultySettings.GetDifficulty(roomIndex)
            : RoomDifficultySnapshot.Default;

        aliveEnemies.Clear();
        droppedGems.Clear();

        waveRoutine = StartCoroutine(RunWaves());
    }

    public void StopCurrentCombat()
    {
        combatRunning = false;

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }

        ClearEnemySubscriptions();
        aliveEnemies.Clear();
        droppedGems.Clear();
    }

    private IEnumerator RunWaves()
    {
        if (currentRoomData == null || currentRoomData.waves == null || currentRoomData.waves.Length == 0)
        {
            FinishRoomCombat();
            yield break;
        }

        foreach (WaveData wave in currentRoomData.waves)
        {
            if (wave == null)
                continue;

            yield return new WaitForSeconds(wave.delayBeforeWave);

            if (wave.enemies == null)
                continue;

            foreach (EnemySpawnEntry entry in wave.enemies)
            {
                if (entry == null || entry.enemyData == null)
                    continue;

                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData);

                    if (entry.delayBetweenSpawns > 0f)
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
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : currentLayout.transform.position;

        GameObject enemyObject = Instantiate(enemyData.prefab, spawnPosition, Quaternion.identity);
        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

        if (enemy == null)
            return;

        enemy.Initialize(enemyData, player);
        enemy.ApplyDifficulty(currentDifficulty);
        TrackEnemy(enemy);
    }

    private void TrackRuntimeEnemy(EnemyBase enemy)
    {
        if (!combatRunning)
            return;

        TrackEnemy(enemy);
    }

    private void TrackEnemy(EnemyBase enemy)
    {
        if (enemy == null || aliveEnemies.Contains(enemy))
            return;

        enemy.OnEnemyDied += HandleEnemyDied;
        aliveEnemies.Add(enemy);
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        enemy.OnEnemyDied -= HandleEnemyDied;
        aliveEnemies.Remove(enemy);
        DropXp(enemy);
    }

    private void DropXp(EnemyBase enemy)
    {
        if (xpGemPool == null || enemy == null || enemy.EnemyData == null)
            return;

        GameObject gemObject = xpGemPool.Get(enemy.transform.position, Quaternion.identity);
        ExperienceGem gem = gemObject != null ? gemObject.GetComponent<ExperienceGem>() : null;

        if (gem == null)
            return;

        gem.Initialize(enemy.EnemyData.xpDropAmount);
        droppedGems.Add(gem);
    }

    private void FinishRoomCombat()
    {
        combatRunning = false;
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

        droppedGems.Clear();
    }

    private Transform GetRandomSpawnPoint()
    {
        if (currentLayout == null || currentLayout.EnemySpawnRoot == null)
            return null;

        Transform root = currentLayout.EnemySpawnRoot;

        if (root.childCount == 0)
            return root;

        return root.GetChild(UnityEngine.Random.Range(0, root.childCount));
    }

    private void ClearEnemySubscriptions()
    {
        foreach (EnemyBase enemy in aliveEnemies)
        {
            if (enemy != null)
                enemy.OnEnemyDied -= HandleEnemyDied;
        }
    }
}
