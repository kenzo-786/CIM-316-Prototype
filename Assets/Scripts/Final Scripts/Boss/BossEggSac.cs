using System;
using System.Collections;
using UnityEngine;

public class BossEggSac : EnemyBase
{
    [SerializeField] private float defaultHatchDelay = 2f;
    [SerializeField] private float pulseSpeed = 8f;
    [SerializeField] private float pulseAmount = 0.08f;

    private EnemyData babySpiderData;
    private RoomDifficultySnapshot babyDifficulty;
    private float hatchDelay;
    private float hatchEndTime;
    private Vector3 originalScale;
    private Coroutine hatchRoutine;
    private bool resolved;

    public event Action<BossEggSac, EnemyBase> OnResolved;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
    }

    public void Configure(
        EnemyData minionData,
        float delay,
        RoomDifficultySnapshot difficulty)
    {
        babySpiderData = minionData;
        hatchDelay = delay > 0f
            ? delay
            : defaultHatchDelay;

        babyDifficulty = difficulty;
        hatchEndTime = Time.time + hatchDelay;
        resolved = false;

        if (hatchRoutine != null)
        {
            StopCoroutine(hatchRoutine);
        }

        hatchRoutine =
            StartCoroutine(HatchRoutine());
    }

    protected override void TickEnemy()
    {
        StopMoving();

        if (resolved)
        {
            return;
        }

        float remaining =
            Mathf.Max(0f, hatchEndTime - Time.time);

        float urgency =
            hatchDelay <= 0f
                ? 1f
                : 1f - Mathf.Clamp01(
                    remaining / hatchDelay
                );

        float pulse =
            Mathf.Sin(
                Time.time *
                Mathf.Lerp(
                    pulseSpeed * 0.5f,
                    pulseSpeed * 2f,
                    urgency
                )
            ) * pulseAmount;

        transform.localScale =
            originalScale * (1f + pulse);
    }

    protected override void OnDeathStarted()
    {
        Resolve(null);
    }

    private IEnumerator HatchRoutine()
    {
        yield return new WaitForSeconds(hatchDelay);

        if (!IsDead)
        {
            EnemyBase minion = SpawnBabySpider();
            Resolve(minion);
            Destroy(gameObject);
        }
    }

    private EnemyBase SpawnBabySpider()
    {
        if (babySpiderData == null ||
            babySpiderData.prefab == null)
        {
            return null;
        }

        Vector2 offset =
            UnityEngine.Random.insideUnitCircle * 0.5f;

        GameObject minionObject =
            Instantiate(
                babySpiderData.prefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );

        EnemyBase minion =
            minionObject.GetComponent<EnemyBase>();

        if (minion == null)
        {
            Destroy(minionObject);
            return null;
        }

        minion.Initialize(
            babySpiderData,
            target
        );

        minion.ApplyDifficulty(babyDifficulty);

        return minion;
    }

    private void Resolve(EnemyBase spawnedMinion)
    {
        if (resolved)
        {
            return;
        }

        resolved = true;

        if (hatchRoutine != null)
        {
            StopCoroutine(hatchRoutine);
            hatchRoutine = null;
        }

        transform.localScale = originalScale;

        OnResolved?.Invoke(
            this,
            spawnedMinion
        );
    }
}
