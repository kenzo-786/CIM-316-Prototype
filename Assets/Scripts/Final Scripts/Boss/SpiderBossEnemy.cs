using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBossEnemy : EnemyBase
{
    private enum BossAttack
    {
        None,
        WebFan,
        VenomCircles,
        WebTraps
    }

    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform fangVisual;
    [SerializeField] private Transform projectileOrigin;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private RoomBounds roomBounds;

    [Header("Attack Prefabs")]
    [SerializeField] private EnemyProjectileData webProjectileData;
    [SerializeField] private GameObject groundHazardPrefab;
    [SerializeField] private GameObject webTrapPrefab;
    [SerializeField] private EnemyData eggSacData;
    [SerializeField] private EnemyData babySpiderData;
    [SerializeField] private Transform[] eggSpawnPoints;

    [Header("Phases")]
    [SerializeField] private float phaseTwoHealth = 0.7f;
    [SerializeField] private float phaseThreeHealth = 0.35f;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 1.5f;
    [SerializeField] private float phaseOneIdleTime = 1.5f;
    [SerializeField] private float phaseTwoIdleTime = 1.15f;
    [SerializeField] private float phaseThreeIdleTime = 0.85f;
    [SerializeField] private float recoveryDuration = 0.75f;

    [Header("Code Animation")]
    [SerializeField] private float windupScaleAmount = 0.08f;
    [SerializeField] private float fangRaiseDistance = 0.35f;

    [Header("Web Fan")]
    [SerializeField] private float webFanWindup = 0.8f;
    [SerializeField] private float webFanSpread = 70f;
    [SerializeField] private int phaseOneProjectileCount = 5;
    [SerializeField] private int phaseTwoProjectileCount = 7;
    [SerializeField] private int phaseThreeProjectileCount = 9;

    [Header("Venom Circles")]
    [SerializeField] private float venomWindup = 0.75f;
    [SerializeField] private int phaseOneVenomCount = 2;
    [SerializeField] private int laterPhaseVenomCount = 3;
    [SerializeField] private float venomTelegraphDuration = 1.25f;
    [SerializeField] private float venomActiveDuration = 2.5f;
    [SerializeField] private float venomDamagePerTick = 5f;
    [SerializeField] private float venomTickInterval = 0.5f;
    [SerializeField] private float venomPlacementRadius = 4f;
    [SerializeField] private float venomPredictionTime = 0.6f;
    [SerializeField] private float venomClearance = 1.3f;

    [Header("Web Traps")]
    [SerializeField] private float webTrapWindup = 0.7f;
    [SerializeField] private int phaseTwoTrapCount = 2;
    [SerializeField] private int phaseThreeTrapCount = 3;
    [SerializeField] private float webTrapDuration = 5f;
    [SerializeField] private float webTrapSpeedMultiplier = 0.7f;
    [SerializeField] private float webTrapPlacementRadius = 5f;
    [SerializeField] private float webTrapClearance = 1f;

    [Header("Egg Sacs")]
    [SerializeField] private float eggSummonWindup = 1f;
    [SerializeField] private int phaseTwoEggCount = 2;
    [SerializeField] private int phaseThreeEggCount = 2;
    [SerializeField] private float eggHatchDelay = 2f;
    [SerializeField] private int maximumLivingMinions = 4;

    [Header("Placement")]
    [SerializeField] private LayerMask blockedLayer;

    private readonly List<GameObject> arenaObjects =
        new List<GameObject>();

    private readonly List<BossEggSac> livingEggs =
        new List<BossEggSac>();

    private readonly List<EnemyBase> livingMinions =
        new List<EnemyBase>();

    private Coroutine attackRoutine;
    private BossAttack previousAttack;
    private bool phaseTwoEggsSummoned;
    private bool phaseThreeEggsSummoned;
    private bool cleaningUp;

    protected override void Awake()
    {
        base.Awake();

        if (visualRoot == null && Visual != null)
        {
            visualRoot = Visual.transform;
        }

        if (projectileOrigin == null)
        {
            projectileOrigin = transform;
        }

        if (telegraph == null)
        {
            telegraph = GetComponent<EnemyTelegraphFeedback>();
        }

        if (roomBounds == null)
        {
            roomBounds = FindObjectOfType<RoomBounds>();
        }
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                base.Initialize(
                    EnemyData,
                    player.transform
                );
            }
        }

        BeginAttackLoop();
    }

    public override void Initialize(
        EnemyData data,
        Transform playerTarget)
    {
        base.Initialize(data, playerTarget);

        phaseTwoEggsSummoned = false;
        phaseThreeEggsSummoned = false;
        previousAttack = BossAttack.None;
        cleaningUp = false;

        if (roomBounds == null)
        {
            roomBounds = FindObjectOfType<RoomBounds>();
        }

        BeginAttackLoop();
    }

    protected override void TickEnemy()
    {
        StopMoving();
    }

    protected override void OnDeathStarted()
    {
        StopAttackLoop();
        CleanupBossObjects();
        base.OnDeathStarted();
    }

    protected override void OnDisable()
    {
        StopAttackLoop();
        CleanupBossObjects();
        base.OnDisable();
    }

    private void BeginAttackLoop()
    {
        if (attackRoutine != null ||
            target == null ||
            IsDead)
        {
            return;
        }

        attackRoutine =
            StartCoroutine(AttackLoop());
    }

    private void StopAttackLoop()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (telegraph != null)
        {
            telegraph.End();
        }
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (!IsDead)
        {
            if (target == null)
            {
                yield return null;
                continue;
            }

            float healthRatio = GetHealthRatio();

            if (!phaseTwoEggsSummoned &&
                healthRatio <= phaseTwoHealth)
            {
                phaseTwoEggsSummoned = true;

                yield return PerformEggSummon(
                    phaseTwoEggCount
                );

                yield return new WaitForSeconds(
                    recoveryDuration
                );

                continue;
            }

            if (!phaseThreeEggsSummoned &&
                healthRatio <= phaseThreeHealth)
            {
                phaseThreeEggsSummoned = true;

                yield return PerformEggSummon(
                    phaseThreeEggCount
                );

                yield return new WaitForSeconds(
                    recoveryDuration
                );

                continue;
            }

            int phase = GetCurrentPhase();

            yield return new WaitForSeconds(
                GetIdleTime(phase)
            );

            if (IsDead)
            {
                break;
            }

            BossAttack selectedAttack =
                SelectAttack(phase);

            previousAttack = selectedAttack;

            switch (selectedAttack)
            {
                case BossAttack.WebFan:
                    yield return PerformWebFan(phase);
                    break;

                case BossAttack.VenomCircles:
                    yield return PerformVenomAttack(phase);
                    break;

                case BossAttack.WebTraps:
                    yield return PerformWebTrapAttack(phase);
                    break;
            }

            yield return new WaitForSeconds(
                recoveryDuration
            );
        }

        attackRoutine = null;
    }

    private BossAttack SelectAttack(int phase)
    {
        int maximumAttack =
            phase >= 2 ? 3 : 2;

        BossAttack selected = previousAttack;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            selected = (BossAttack)Random.Range(
                1,
                maximumAttack + 1
            );

            if (selected != previousAttack)
            {
                break;
            }
        }

        return selected;
    }

    private IEnumerator PerformWebFan(int phase)
    {
        yield return PlayCodeTelegraph(
            webFanWindup,
            false
        );

        if (webProjectileData == null ||
            webProjectileData.prefab == null ||
            target == null)
        {
            yield break;
        }

        int projectileCount =
            phase == 1
                ? phaseOneProjectileCount
                : phase == 2
                    ? phaseTwoProjectileCount
                    : phaseThreeProjectileCount;

        Vector2 origin =
            projectileOrigin != null
                ? projectileOrigin.position
                : transform.position;

        Vector2 baseDirection =
            ((Vector2)target.position - origin)
            .normalized;

        if (baseDirection == Vector2.zero)
        {
            baseDirection = Vector2.down;
        }

        for (int i = 0; i < projectileCount; i++)
        {
            float interpolation =
                projectileCount <= 1
                    ? 0.5f
                    : (float)i /
                      (projectileCount - 1);

            float angle =
                Mathf.Lerp(
                    -webFanSpread * 0.5f,
                    webFanSpread * 0.5f,
                    interpolation
                );

            Vector2 direction =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                ) * baseDirection;

            SpawnWebProjectile(
                origin,
                direction
            );
        }
    }

    private void SpawnWebProjectile(
        Vector2 origin,
        Vector2 direction)
    {
        GameObject projectileObject =
            ProjectilePoolProvider.Instance != null
                ? ProjectilePoolProvider.Instance.Spawn(
                    webProjectileData.prefab,
                    origin,
                    Quaternion.identity
                )
                : Instantiate(
                    webProjectileData.prefab,
                    origin,
                    Quaternion.identity
                );

        EnemyProjectile projectile =
            projectileObject != null
                ? projectileObject
                    .GetComponent<EnemyProjectile>()
                : null;

        if (projectile == null)
        {
            if (projectileObject != null)
            {
                Destroy(projectileObject);
            }

            return;
        }

        projectile.Launch(
            webProjectileData,
            direction,
            CurrentDifficulty.damageMultiplier,
            gameObject
        );
    }

    private IEnumerator PerformVenomAttack(int phase)
    {
        yield return PlayCodeTelegraph(
            venomWindup,
            true
        );

        if (groundHazardPrefab == null ||
            target == null)
        {
            yield break;
        }

        int count =
            phase == 1
                ? phaseOneVenomCount
                : laterPhaseVenomCount;

        List<Vector2> usedPositions =
            new List<Vector2>();

        for (int i = 0; i < count; i++)
        {
            Vector2 position =
                FindArenaPosition(
                    i,
                    venomPlacementRadius,
                    venomClearance,
                    true,
                    usedPositions
                );

            usedPositions.Add(position);

            GameObject hazardObject =
                Instantiate(
                    groundHazardPrefab,
                    position,
                    Quaternion.identity
                );

            BossGroundHazard hazard =
                hazardObject.GetComponent
                    <BossGroundHazard>();

            if (hazard == null)
            {
                Destroy(hazardObject);
                continue;
            }

            arenaObjects.Add(hazardObject);

            hazard.Initialize(
                gameObject,
                venomTelegraphDuration,
                venomActiveDuration,
                venomDamagePerTick *
                CurrentDifficulty.damageMultiplier,
                venomTickInterval
            );
        }
    }

    private IEnumerator PerformWebTrapAttack(int phase)
    {
        yield return PlayCodeTelegraph(
            webTrapWindup,
            false
        );

        if (webTrapPrefab == null ||
            target == null)
        {
            yield break;
        }

        int count =
            phase == 2
                ? phaseTwoTrapCount
                : phaseThreeTrapCount;

        List<Vector2> usedPositions =
            new List<Vector2>();

        for (int i = 0; i < count; i++)
        {
            Vector2 position =
                FindArenaPosition(
                    i,
                    webTrapPlacementRadius,
                    webTrapClearance,
                    false,
                    usedPositions
                );

            usedPositions.Add(position);

            GameObject trapObject =
                Instantiate(
                    webTrapPrefab,
                    position,
                    Quaternion.identity
                );

            BossWebTrap trap =
                trapObject.GetComponent<BossWebTrap>();

            if (trap == null)
            {
                Destroy(trapObject);
                continue;
            }

            arenaObjects.Add(trapObject);

            trap.Initialize(
                webTrapDuration,
                webTrapSpeedMultiplier
            );
        }
    }

    private IEnumerator PerformEggSummon(int eggCount)
    {
        yield return PlayCodeTelegraph(
            eggSummonWindup,
            false
        );

        if (eggSacData == null ||
            eggSacData.prefab == null)
        {
            yield break;
        }

        for (int i = 0; i < eggCount; i++)
        {
            Vector2 position = GetEggSpawnPosition(i);

            GameObject eggObject =
                Instantiate(
                    eggSacData.prefab,
                    position,
                    Quaternion.identity
                );

            BossEggSac egg =
                eggObject.GetComponent<BossEggSac>();

            if (egg == null)
            {
                Destroy(eggObject);
                continue;
            }

            egg.OnResolved += HandleEggResolved;

            egg.Initialize(
                eggSacData,
                target
            );

            egg.ApplyDifficulty(CurrentDifficulty);

            egg.Configure(
                babySpiderData,
                eggHatchDelay,
                CurrentDifficulty
            );

            livingEggs.Add(egg);
        }
    }

    private Vector2 GetEggSpawnPosition(int index)
    {
        if (eggSpawnPoints != null &&
            eggSpawnPoints.Length > 0)
        {
            Transform point =
                eggSpawnPoints[
                    index %
                    eggSpawnPoints.Length
                ];

            if (point != null)
            {
                return point.position;
            }
        }

        Vector2 position =
            (Vector2)transform.position +
            Random.insideUnitCircle * 3f;

        return roomBounds != null
            ? roomBounds.ClampPoint(position)
            : position;
    }

    private void HandleEggResolved(
        BossEggSac egg,
        EnemyBase spawnedMinion)
    {
        if (egg != null)
        {
            egg.OnResolved -= HandleEggResolved;
            livingEggs.Remove(egg);
        }

        RemoveMissingMinions();

        if (spawnedMinion == null)
        {
            return;
        }

        if (livingMinions.Count >=
            maximumLivingMinions)
        {
            Destroy(spawnedMinion.gameObject);
            return;
        }

        livingMinions.Add(spawnedMinion);

        spawnedMinion.OnEnemyDied +=
            HandleMinionDied;
    }

    private void HandleMinionDied(EnemyBase minion)
    {
        if (minion != null)
        {
            minion.OnEnemyDied -= HandleMinionDied;
        }

        livingMinions.Remove(minion);
    }

    private void RemoveMissingMinions()
    {
        for (int i = livingMinions.Count - 1;
             i >= 0;
             i--)
        {
            if (livingMinions[i] == null ||
                livingMinions[i].IsDead)
            {
                livingMinions.RemoveAt(i);
            }
        }
    }

    private Vector2 FindArenaPosition(
        int index,
        float searchRadius,
        float clearance,
        bool targetFirst,
        List<Vector2> usedPositions)
    {
        Vector2 playerPosition =
            target != null
                ? target.position
                : transform.position;

        Rigidbody2D playerBody =
            target != null
                ? target.GetComponent<Rigidbody2D>()
                : null;

        for (int attempt = 0; attempt < 15; attempt++)
        {
            Vector2 candidate;

            if (attempt == 0 &&
                targetFirst &&
                index == 0)
            {
                candidate = playerPosition;
            }
            else if (attempt == 0 &&
                     targetFirst &&
                     index == 1 &&
                     playerBody != null)
            {
                candidate =
                    playerPosition +
                    playerBody.linearVelocity *
                    venomPredictionTime;
            }
            else
            {
                candidate =
                    playerPosition +
                    Random.insideUnitCircle *
                    searchRadius;
            }

            if (roomBounds != null)
            {
                candidate =
                    roomBounds.ClampPoint(candidate);
            }

            bool blocked =
                Physics2D.OverlapCircle(
                    candidate,
                    clearance,
                    blockedLayer
                );

            if (blocked)
            {
                continue;
            }

            bool tooClose = false;

            foreach (Vector2 usedPosition
                     in usedPositions)
            {
                if (Vector2.Distance(
                        candidate,
                        usedPosition) <
                    clearance * 1.8f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                return candidate;
            }
        }

        return roomBounds != null
            ? roomBounds.GetRandomPoint()
            : playerPosition;
    }

    private IEnumerator PlayCodeTelegraph(
        float duration,
        bool raiseFangs)
    {
        duration = Mathf.Max(0.1f, duration);

        if (telegraph != null)
        {
            telegraph.Begin(duration);
        }

        Vector3 startingScale =
            visualRoot != null
                ? visualRoot.localScale
                : Vector3.one;

        Vector3 startingFangPosition =
            fangVisual != null
                ? fangVisual.localPosition
                : Vector3.zero;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            float arc =
                Mathf.Sin(progress * Mathf.PI);

            if (visualRoot != null)
            {
                visualRoot.localScale =
                    startingScale *
                    (1f +
                     arc * windupScaleAmount);
            }

            if (raiseFangs &&
                fangVisual != null)
            {
                fangVisual.localPosition =
                    startingFangPosition +
                    Vector3.up *
                    fangRaiseDistance *
                    arc;
            }

            yield return null;
        }

        if (visualRoot != null)
        {
            visualRoot.localScale =
                startingScale;
        }

        if (fangVisual != null)
        {
            fangVisual.localPosition =
                startingFangPosition;
        }

        if (telegraph != null)
        {
            telegraph.End();
        }
    }

    private int GetCurrentPhase()
    {
        float ratio = GetHealthRatio();

        if (ratio <= phaseThreeHealth)
        {
            return 3;
        }

        if (ratio <= phaseTwoHealth)
        {
            return 2;
        }

        return 1;
    }

    private float GetHealthRatio()
    {
        if (health == null ||
            health.MaxHealth <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(
            health.CurrentHealth /
            health.MaxHealth
        );
    }

    private float GetIdleTime(int phase)
    {
        switch (phase)
        {
            case 2:
                return phaseTwoIdleTime;

            case 3:
                return phaseThreeIdleTime;

            default:
                return phaseOneIdleTime;
        }
    }

    private void CleanupBossObjects()
    {
        if (cleaningUp)
        {
            return;
        }

        cleaningUp = true;

        foreach (GameObject arenaObject
                 in arenaObjects)
        {
            if (arenaObject != null)
            {
                Destroy(arenaObject);
            }
        }

        arenaObjects.Clear();

        foreach (BossEggSac egg in livingEggs)
        {
            if (egg != null)
            {
                egg.OnResolved -= HandleEggResolved;
                Destroy(egg.gameObject);
            }
        }

        livingEggs.Clear();

        foreach (EnemyBase minion in livingMinions)
        {
            if (minion != null)
            {
                minion.OnEnemyDied -= HandleMinionDied;
                Destroy(minion.gameObject);
            }
        }

        livingMinions.Clear();

        EnemyProjectile[] projectiles =
            FindObjectsOfType<EnemyProjectile>();

        foreach (EnemyProjectile projectile
                 in projectiles)
        {
            if (projectile != null &&
                projectile.gameObject
                    .activeInHierarchy)
            {
                PooledProjectileUtility.Despawn(
                    projectile.gameObject
                );
            }
        }
    }
}
