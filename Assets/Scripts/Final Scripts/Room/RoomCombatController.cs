using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomCombatController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private ObjectPool xpGemPool;
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private RoomDifficultySettings difficultySettings;

    [Header("Player")]
    [SerializeField] private PlayerWeaponController playerWeaponController;

    private readonly List<EnemyBase> aliveEnemies = new List<EnemyBase>();
    private readonly List<ExperienceGem> droppedGems = new List<ExperienceGem>();
    private readonly List<EnemySpawnEntry> pendingSpawns = new List<EnemySpawnEntry>();

    private RoomData currentRoomData;
    private RoomLayout currentLayout;
    private Transform player;

    private RoomDifficultySnapshot currentDifficulty =
        RoomDifficultySnapshot.Default;

    private Coroutine waveRoutine;
    private bool combatRunning;

    public event Action<int, int, float> OnWaveWarning;
    public event Action<int, int> OnWaveStarted;
    public event Action OnRoomCombatCleared;
    public event Action OnRoomCombatStopped;

    public bool CombatRunning => combatRunning;
    public int AliveEnemyCount => aliveEnemies.Count;

    private void OnEnable()
    {
        EnemyRuntimeRegistry.OnEnemySpawnedRuntime += TrackRuntimeEnemy;
    }

    private void OnDisable()
    {
        EnemyRuntimeRegistry.OnEnemySpawnedRuntime -= TrackRuntimeEnemy;

        StopAllCoroutines();
        ClearEnemySubscriptions();
        SetPlayerCombatActive(false);
        DespawnAllCombatProjectiles();
    }

    public void StartRoomCombat(
        RoomData roomData,
        RoomLayout layout,
        Transform playerTransform)
    {
        StartRoomCombat(roomData, layout, playerTransform, 0);
    }

    public void StartRoomCombat(
        RoomData roomData,
        RoomLayout layout,
        Transform playerTransform,
        int roomIndex)
    {
        StopCurrentCombat();

        currentRoomData = roomData;
        currentLayout = layout;
        player = playerTransform;

        if (playerWeaponController == null && player != null)
        {
            playerWeaponController =
                player.GetComponent<PlayerWeaponController>();
        }

        currentDifficulty =
            difficultySettings != null
                ? difficultySettings.GetDifficulty(roomIndex)
                : RoomDifficultySnapshot.Default;

        combatRunning = true;
        aliveEnemies.Clear();
        droppedGems.Clear();

        SetPlayerCombatActive(false);

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

        SetPlayerCombatActive(false);
        DespawnAllCombatProjectiles();

        ClearEnemySubscriptions();
        aliveEnemies.Clear();
        droppedGems.Clear();

        OnRoomCombatStopped?.Invoke();
    }

    private IEnumerator RunWaves()
    {
        if (currentRoomData == null ||
            currentRoomData.waves == null ||
            currentRoomData.waves.Length == 0)
        {
            FinishRoomCombat();
            yield break;
        }

        int totalWaves = CountValidWaves();

        if (totalWaves == 0)
        {
            FinishRoomCombat();
            yield break;
        }

        int currentWaveNumber = 0;

        foreach (WaveData wave in currentRoomData.waves)
        {
            if (wave == null)
            {
                continue;
            }

            currentWaveNumber++;

            SetPlayerCombatActive(false);
            DespawnAllCombatProjectiles();

            float warningDuration =
                Mathf.Max(0f, wave.delayBeforeWave);

            OnWaveWarning?.Invoke(
                currentWaveNumber,
                totalWaves,
                warningDuration
            );

            if (warningDuration > 0f)
            {
                yield return new WaitForSeconds(warningDuration);
            }

            if (!combatRunning)
            {
                yield break;
            }

            SetPlayerCombatActive(true);

            OnWaveStarted?.Invoke(
                currentWaveNumber,
                totalWaves
            );

            yield return SpawnWaveEnemies(wave);

            yield return new WaitUntil(
                () => !combatRunning || aliveEnemies.Count == 0
            );

            if (!combatRunning)
            {
                yield break;
            }
        }

        FinishRoomCombat();
    }

    private IEnumerator SpawnWaveEnemies(WaveData wave)
    {
        if (wave.enemies == null)
        {
            yield break;
        }

        pendingSpawns.Clear();

        foreach (EnemySpawnEntry entry in wave.enemies)
        {
            if (entry == null || entry.enemyData == null)
            {
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                pendingSpawns.Add(entry);
            }
        }

        while (pendingSpawns.Count > 0)
        {
            if (!combatRunning)
            {
                yield break;
            }

            int index = UnityEngine.Random.Range(0, pendingSpawns.Count);
            EnemySpawnEntry entry = pendingSpawns[index];
            pendingSpawns.RemoveAt(index);

            SpawnEnemy(entry.enemyData);

            if (pendingSpawns.Count > 0 && entry.delayBetweenSpawns > 0f)
            {
                yield return new WaitForSeconds(entry.delayBetweenSpawns);
            }
        }
    }

    private int CountValidWaves()
    {
        int count = 0;

        foreach (WaveData wave in currentRoomData.waves)
        {
            if (wave != null)
            {
                count++;
            }
        }

        return count;
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            return;
        }

        Transform spawnPoint = GetRandomSpawnPoint();

        Vector3 spawnPosition =
            spawnPoint != null
                ? spawnPoint.position
                : currentLayout.transform.position;

        GameObject enemyObject = Instantiate(
            enemyData.prefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyBase enemy =
            enemyObject.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            Destroy(enemyObject);
            return;
        }

        enemy.Initialize(enemyData, player);
        enemy.ApplyDifficulty(currentDifficulty);

        TrackEnemy(enemy);
    }

    private void TrackRuntimeEnemy(EnemyBase enemy)
    {
        if (!combatRunning)
        {
            return;
        }

        TrackEnemy(enemy);
    }

    private void TrackEnemy(EnemyBase enemy)
    {
        if (enemy == null || aliveEnemies.Contains(enemy))
        {
            return;
        }

        enemy.OnEnemyDied += HandleEnemyDied;
        aliveEnemies.Add(enemy);
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.OnEnemyDied -= HandleEnemyDied;
        aliveEnemies.Remove(enemy);

        DropXp(enemy);
    }

    private void DropXp(EnemyBase enemy)
    {
        if (xpGemPool == null ||
            enemy == null ||
            enemy.EnemyData == null)
        {
            return;
        }

        GameObject gemObject = xpGemPool.Get(
            enemy.transform.position,
            Quaternion.identity
        );

        ExperienceGem gem =
            gemObject != null
                ? gemObject.GetComponent<ExperienceGem>()
                : null;

        if (gem == null)
        {
            return;
        }

        gem.Initialize(enemy.EnemyData.xpDropAmount);
        droppedGems.Add(gem);
    }

    private void FinishRoomCombat()
    {
        if (!combatRunning)
        {
            return;
        }

        combatRunning = false;
        waveRoutine = null;

        SetPlayerCombatActive(false);
        DespawnAllCombatProjectiles();
        MagnetizeAllGems();

        OnRoomCombatCleared?.Invoke();
    }

    private void SetPlayerCombatActive(bool active)
    {
        if (playerWeaponController == null && player != null)
        {
            playerWeaponController =
                player.GetComponent<PlayerWeaponController>();
        }

        if (playerWeaponController != null)
        {
            playerWeaponController.SetCombatActive(active);
        }
    }

    private void DespawnAllCombatProjectiles()
    {
        EraserProjectile[] playerProjectiles =
            FindObjectsOfType<EraserProjectile>();

        foreach (EraserProjectile projectile in playerProjectiles)
        {
            if (projectile != null &&
                projectile.gameObject.activeInHierarchy)
            {
                PooledProjectileUtility.Despawn(
                    projectile.gameObject
                );
            }
        }

        EnemyProjectile[] enemyProjectiles =
            FindObjectsOfType<EnemyProjectile>();

        foreach (EnemyProjectile projectile in enemyProjectiles)
        {
            if (projectile != null &&
                projectile.gameObject.activeInHierarchy)
            {
                PooledProjectileUtility.Despawn(
                    projectile.gameObject
                );
            }
        }
    }

    private void MagnetizeAllGems()
    {
        foreach (ExperienceGem gem in droppedGems)
        {
            if (gem != null)
            {
                gem.MagnetizeTo(player, playerExperience);
            }
        }

        droppedGems.Clear();
    }

    private Transform GetRandomSpawnPoint()
    {
        if (currentLayout == null ||
            currentLayout.EnemySpawnRoot == null)
        {
            return null;
        }

        Transform root = currentLayout.EnemySpawnRoot;

        if (root.childCount == 0)
        {
            return root;
        }

        return root.GetChild(
            UnityEngine.Random.Range(0, root.childCount)
        );
    }

    private void ClearEnemySubscriptions()
    {
        foreach (EnemyBase enemy in aliveEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDied -= HandleEnemyDied;
            }
        }
    }
}
