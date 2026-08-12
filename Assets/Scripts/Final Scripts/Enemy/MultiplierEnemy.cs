using UnityEngine;

public class MultiplierEnemy : EnemyBase
{
    [Header("Multiplier")]
    [SerializeField] private int maxSplitGenerations = 2;
    [SerializeField] private float childSpawnRadius = 1.25f;
    [SerializeField] private float childScaleMultiplier = 0.75f;
    [SerializeField] private float childRevealDuration = 0.22f;

    [Header("Attack")]
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private float attackRangeLeeway = 0.25f;
    [SerializeField] private EnemyAnimationController animationController;

    private float nextAttackTime;
    private float attackTimer;
    private int splitGeneration;
    private bool attacking;
    private Vector2 attackDirection = Vector2.down;

    protected override void Awake()
    {
        base.Awake();

        if (animationController == null)
            animationController = GetComponentInChildren<EnemyAnimationController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        attacking = false;
        nextAttackTime = 0f;
        animationController?.SetStationary();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (attacking)
        {
            StopMoving();
            animationController?.SetStationary();
            animationController?.SetFacingDirection(attackDirection);

            attackTimer -= Time.fixedDeltaTime;

            if (attackTimer <= 0f)
                FinishAttack();

            return;
        }

        Vector2 toPlayer = (Vector2)target.position - rb.position;

        if (IsTargetInRange(AttackRange))
        {
            StopMoving();
            animationController?.SetStationary();
            animationController?.SetFacingDirection(toPlayer);
            TryStartAttack(toPlayer);
            return;
        }

        MoveInDirection(toPlayer);
        animationController?.SetMovementDirection(toPlayer);
    }

    private void TryStartAttack(Vector2 toPlayer)
    {
        if (Time.time < nextAttackTime)
            return;

        if (toPlayer.sqrMagnitude > 0.0001f)
            attackDirection = toPlayer.normalized;

        attacking = true;
        attackTimer = attackWindup;
        nextAttackTime = Time.time + AttackCooldown;

        animationController?.SetStationary();
        animationController?.SetFacingDirection(attackDirection);
        animationController?.PlayAttack();
    }

    private void FinishAttack()
    {
        attacking = false;

        if (!IsTargetInRange(AttackRange + attackRangeLeeway))
            return;

        DamageTarget(ContactDamage, target.position);
    }

    protected override void OnDeathStarted()
    {
        base.OnDeathStarted();
        SpawnChildren();
    }

    private void SpawnChildren()
    {
        if (splitGeneration >= maxSplitGenerations || EnemyData == null)
            return;

        EnemyData childData = EnemyData.childEnemyData;
        int spawnCount = EnemyData.childCount;

        if (childData == null || childData.prefab == null || spawnCount <= 0)
            return;

        if (childData == EnemyData)
        {
            Debug.LogError("Multiplier EnemyData references itself: " + EnemyData.name, this);
            return;
        }

        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = startAngle + 360f * i / spawnCount;
            float radians = angle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            );

            Vector2 spawnPosition = (Vector2)transform.position + direction * childSpawnRadius;

            GameObject childObject = Instantiate(
                childData.prefab,
                spawnPosition,
                Quaternion.identity
            );

            Vector3 finalChildScale = transform.localScale * childScaleMultiplier;
            childObject.transform.localScale = finalChildScale;

            EnemyBase childEnemy = childObject.GetComponent<EnemyBase>();

            if (childEnemy == null)
            {
                Destroy(childObject);
                continue;
            }

            if (childEnemy is MultiplierEnemy multiplierChild)
                multiplierChild.splitGeneration = splitGeneration + 1;

            childEnemy.Initialize(childData, target);
            childEnemy.ApplyDifficulty(CurrentDifficulty);

            SpawnScaleReveal reveal = childObject.AddComponent<SpawnScaleReveal>();
            reveal.Play(finalChildScale, childRevealDuration);

            EnemyRuntimeRegistry.RaiseEnemySpawned(childEnemy);
        }
    }
}
