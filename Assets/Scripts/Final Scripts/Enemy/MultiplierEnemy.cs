using UnityEngine;

public class MultiplierEnemy : EnemyBase
{
    [Header("Multiplier")]
    [SerializeField] private EnemyData childEnemyData;
    [SerializeField] private int childCount = 2;
    [SerializeField] private float childSpawnRadius = 0.6f;
    [SerializeField] private float childScaleMultiplier = 0.75f;

    private float nextAttackTime;

    protected override void TickEnemy()
    {
        if (target == null)
            return;

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
            return;

        nextAttackTime = Time.time + AttackCooldown;
        DamageTarget(ContactDamage, target.position);
    }

    protected override void OnDeathStarted()
    {
        base.OnDeathStarted();
        SpawnChildren();
    }

    private void SpawnChildren()
    {
        EnemyData dataToSpawn = childEnemyData != null ? childEnemyData : EnemyData != null ? EnemyData.childEnemyData : null;
        int spawnCount = childCount > 0 ? childCount : EnemyData != null ? EnemyData.childCount : 0;

        if (dataToSpawn == null || dataToSpawn.prefab == null || spawnCount <= 0)
            return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * childSpawnRadius;

            if (offset == Vector2.zero)
                offset = Vector2.right * childSpawnRadius;

            GameObject childObject = Instantiate(dataToSpawn.prefab, (Vector2)transform.position + offset, Quaternion.identity);
            childObject.transform.localScale = transform.localScale * childScaleMultiplier;

            EnemyBase childEnemy = childObject.GetComponent<EnemyBase>();
            if (childEnemy == null)
                continue;

            childEnemy.Initialize(dataToSpawn, target);
            childEnemy.ApplyDifficulty(CurrentDifficulty);
            EnemyRuntimeRegistry.RaiseEnemySpawned(childEnemy);
        }
    }
}
