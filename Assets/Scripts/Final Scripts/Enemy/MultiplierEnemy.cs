using UnityEngine;

public class MultiplierEnemy : EnemyBase
{
    [Header("Multiplier")]
    [SerializeField] private EnemyData childEnemyData;
    [SerializeField] private int generation;
    [SerializeField] private int maxGeneration = 2;
    [SerializeField] private int childrenPerDeath = 2;
    [SerializeField] private float childSpawnOffset = 0.7f;
    [SerializeField] private float childScaleMultiplier = 0.65f;
    [SerializeField] private float childHealthMultiplier = 0.6f;
    [SerializeField] private float childSpeedMultiplier = 1.2f;

    protected override void HandleDied()
    {
        SpawnChildren();
        base.HandleDied();
    }

    private void SpawnChildren()
    {
        if (generation >= maxGeneration)
            return;

        EnemyData dataToSpawn = childEnemyData != null ? childEnemyData : enemyData;

        if (dataToSpawn == null || dataToSpawn.prefab == null)
            return;

        for (int i = 0; i < childrenPerDeath; i++)
        {
            float angle = (360f / childrenPerDeath) * i;
            Vector2 offset = Quaternion.Euler(0f, 0f, angle) * Vector2.right * childSpawnOffset;

            GameObject childObject = Instantiate(
                dataToSpawn.prefab,
                transform.position + (Vector3)offset,
                Quaternion.identity
            );

            childObject.transform.localScale = transform.localScale * childScaleMultiplier;

            MultiplierEnemy childMultiplier = childObject.GetComponent<MultiplierEnemy>();
            if (childMultiplier != null)
            {
                childMultiplier.SetGeneration(generation + 1);
                childMultiplier.Initialize(dataToSpawn, target);
            }

            Health childHealth = childObject.GetComponent<Health>();
            if (childHealth != null)
            {
                childHealth.SetMaxHealth(enemyData.maxHealth * childHealthMultiplier, true);
            }

            EnemyBase childEnemy = childObject.GetComponent<EnemyBase>();
            if (childEnemy != null && childMultiplier == null)
            {
                childEnemy.Initialize(dataToSpawn, target);
            }

            Rigidbody2D childRb = childObject.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                childRb.linearVelocity = offset.normalized * enemyData.moveSpeed * childSpeedMultiplier;
            }
        }
    }

    public void SetGeneration(int value)
    {
        generation = value;
    }
}
