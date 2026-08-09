using UnityEngine;

public class MultiplierEnemy : EnemyBase
{
    [Header("Multiplier")]
    [SerializeField] private int maxSplitGenerations = 2;
    [SerializeField] private float childSpawnRadius = 0.6f;
    [SerializeField] private float childScaleMultiplier = 0.75f;
    [SerializeField] private float childRevealDuration = 0.22f;

    private float nextAttackTime;
    private int splitGeneration;

    protected override void TickEnemy()
    {
        if (target == null)
        {
            return;
        }

        if (IsTargetInRange(AttackRange))
        {
            StopMoving();
            TryAttack();
            return;
        }

        MoveToward(target.position);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + AttackCooldown;

        DamageTarget(
            ContactDamage,
            target.position
        );
    }

    protected override void OnDeathStarted()
    {
        base.OnDeathStarted();
        SpawnChildren();
    }

    private void SpawnChildren()
    {
        if (splitGeneration >= maxSplitGenerations)
        {
            return;
        }

        if (EnemyData == null)
        {
            return;
        }

        EnemyData childData =
            EnemyData.childEnemyData;

        int spawnCount =
            EnemyData.childCount;

        if (childData == null ||
            childData.prefab == null ||
            spawnCount <= 0)
        {
            return;
        }

        if (childData == EnemyData)
        {
            Debug.LogError(
                "Multiplier EnemyData references itself: " +
                EnemyData.name,
                this
            );

            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 direction =
                Random.insideUnitCircle.normalized;

            if (direction == Vector2.zero)
            {
                direction = Vector2.right;
            }

            Vector2 spawnPosition =
                (Vector2)transform.position +
                direction *
                childSpawnRadius;

            GameObject childObject =
                Instantiate(
                    childData.prefab,
                    spawnPosition,
                    Quaternion.identity
                );

            Vector3 finalChildScale =
                transform.localScale *
                childScaleMultiplier;

            childObject.transform.localScale =
                finalChildScale;

            EnemyBase childEnemy =
                childObject.GetComponent<EnemyBase>();

            if (childEnemy == null)
            {
                Debug.LogError(
                    "Multiplier child prefab is missing EnemyBase.",
                    childObject
                );

                Destroy(childObject);
                continue;
            }

            if (childEnemy is MultiplierEnemy multiplierChild)
            {
                multiplierChild.splitGeneration =
                    splitGeneration + 1;
            }

            childEnemy.Initialize(
                childData,
                target
            );

            childEnemy.ApplyDifficulty(
                CurrentDifficulty
            );

            SpawnScaleReveal reveal =
                childObject.AddComponent<SpawnScaleReveal>();

            reveal.Play(
                finalChildScale,
                childRevealDuration
            );

            EnemyRuntimeRegistry.RaiseEnemySpawned(
                childEnemy
            );
        }
    }
}
