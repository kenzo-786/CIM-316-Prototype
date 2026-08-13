using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private SpriteRenderer visual;

    [Header("Movement Steering")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField, Min(0f)] private float separationRadius = 1.4f;
    [SerializeField, Min(0f)] private float separationStrength = 2.25f;
    [SerializeField, Min(0f)] private float separationPadding = 0.18f;
    [SerializeField, Min(0f)] private float obstacleProbeDistance = 0.8f;
    [SerializeField, Min(0f)] private float obstacleProbeRadius = 0.3f;
    [SerializeField, Min(0f)] private float wallAvoidanceStrength = 1.5f;
    [SerializeField, Min(0f)] private float movementSkin = 0.03f;

    protected Rigidbody2D rb;
    protected Health health;
    protected Transform target;

    private bool isDead;
    private EnemyDeathFeedback deathFeedback;
    private CodeDeathAnimation deathAnimation;
    private RoomDifficultySnapshot currentDifficulty = RoomDifficultySnapshot.Default;
    private float bodyRadius = 0.4f;

    public event Action<EnemyBase> OnEnemyDied;

    public EnemyData EnemyData => enemyData;
    public Transform Target => target;
    public RoomDifficultySnapshot CurrentDifficulty => currentDifficulty;
    public bool IsDead => isDead;
    public float BodyRadius => bodyRadius;

    protected float MoveSpeed =>
        enemyData != null
            ? enemyData.moveSpeed * currentDifficulty.moveSpeedMultiplier
            : 0f;

    protected float ContactDamage =>
        enemyData != null
            ? enemyData.contactDamage * currentDifficulty.damageMultiplier
            : 0f;

    protected float AttackRange =>
        enemyData != null
            ? enemyData.attackRange
            : 1f;

    protected float AttackCooldown =>
        enemyData != null
            ? enemyData.attackCooldown
            : 1f;

    protected SpriteRenderer Visual => visual;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        deathFeedback = GetComponent<EnemyDeathFeedback>();
        deathAnimation = GetComponent<CodeDeathAnimation>();

        if (visual == null)
            visual = GetComponentInChildren<SpriteRenderer>();

        if (enemyLayer.value == 0)
            enemyLayer = LayerMask.GetMask("Enemy");

        if (obstacleLayer.value == 0)
            obstacleLayer = LayerMask.GetMask(
                "Wall",
                "Door",
                "Obstacle"
            );

        Collider2D bodyCollider = GetComponent<Collider2D>();

        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            bodyRadius = Mathf.Max(
                bounds.extents.x,
                bounds.extents.y
            );
        }

        bodyRadius = Mathf.Max(bodyRadius, 0.2f);
    }

    protected virtual void OnEnable()
    {
        isDead = false;

        if (health != null)
            health.OnDied += HandleDied;
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    public virtual void Initialize(
        EnemyData data,
        Transform playerTarget)
    {
        enemyData = data;
        target = playerTarget;
        isDead = false;

        if (enemyData != null && health != null)
            health.SetMaxHealth(enemyData.maxHealth, true);
    }

    public virtual void ApplyDifficulty(
        RoomDifficultySnapshot difficulty)
    {
        currentDifficulty = difficulty;

        if (enemyData != null && health != null)
        {
            health.SetMaxHealth(
                enemyData.maxHealth *
                currentDifficulty.healthMultiplier,
                true
            );
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || target == null || enemyData == null)
            return;

        TickEnemy();
    }

    protected abstract void TickEnemy();

    protected void MoveToward(Vector2 worldPosition)
    {
        MoveInDirection(worldPosition - rb.position);
    }

    protected void MoveAwayFrom(Vector2 worldPosition)
    {
        MoveInDirection(rb.position - worldPosition);
    }

    protected void MoveInDirection(
        Vector2 direction,
        float speedMultiplier = 1f)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMoving();
            return;
        }

        Vector2 steeringDirection = GetSteeredDirection(direction);

        if (steeringDirection.sqrMagnitude <= 0.0001f)
        {
            StopMoving();
            return;
        }

        float moveDistance =
            MoveSpeed *
            speedMultiplier *
            Time.fixedDeltaTime;

        MoveWithCollision(steeringDirection, moveDistance);
        FaceDirection(steeringDirection);
    }

    protected Vector2 GetSteeredDirection(
        Vector2 desiredDirection)
    {
        Vector2 desired = desiredDirection.normalized;

        if (desired == Vector2.zero)
            return Vector2.zero;

        Vector2 separation = CalculateSeparation();

        Vector2 steering =
            desired +
            separation * separationStrength;

        if (steering.sqrMagnitude <= 0.0001f)
            steering = desired;

        steering.Normalize();

        if (obstacleLayer.value == 0 ||
            obstacleProbeDistance <= 0f)
        {
            return steering;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            rb.position,
            obstacleProbeRadius,
            steering,
            obstacleProbeDistance,
            obstacleLayer
        );

        if (hit.collider == null)
            return steering;

        Vector2 left =
            new Vector2(-hit.normal.y, hit.normal.x);

        Vector2 right = -left;

        Vector2 sideStep =
            Vector2.Dot(left, desired) >
            Vector2.Dot(right, desired)
                ? left
                : right;

        Vector2 redirected =
            desired * 0.2f +
            sideStep * wallAvoidanceStrength +
            separation * separationStrength;

        return redirected.sqrMagnitude > 0.0001f
            ? redirected.normalized
            : sideStep;
    }

    protected void StopMoving()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    protected bool IsTargetInRange(float range)
    {
        return target != null &&
               Vector2.Distance(
                   transform.position,
                   target.position
               ) <= range;
    }

    protected void DamageTarget(
        float damage,
        Vector2 hitPoint)
    {
        if (target == null || damage <= 0f)
            return;

        IDamageable damageable =
            target.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(
                new DamageInfo(
                    damage,
                    gameObject,
                    hitPoint
                )
            );
        }
    }

    protected IDamageable GetDamageable(Collider2D hit)
    {
        if (hit == null)
            return null;

        IDamageable damageable =
            hit.GetComponent<IDamageable>();

        if (damageable == null)
            damageable =
                hit.GetComponentInParent<IDamageable>();

        return damageable;
    }

    protected void FaceDirection(Vector2 direction)
    {
        if (visual == null ||
            Mathf.Abs(direction.x) < 0.01f)
        {
            return;
        }

        visual.flipX = direction.x < 0f;
    }

    private void MoveWithCollision(
        Vector2 direction,
        float distance)
    {
        if (rb == null || distance <= 0f)
            return;

        if (obstacleLayer.value == 0)
        {
            rb.MovePosition(
                rb.position + direction * distance
            );

            return;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            rb.position,
            bodyRadius,
            direction,
            distance + movementSkin,
            obstacleLayer
        );

        if (hit.collider == null)
        {
            rb.MovePosition(
                rb.position + direction * distance
            );

            return;
        }

        float safeDistance = Mathf.Max(
            0f,
            hit.distance - movementSkin
        );

        if (safeDistance > 0f)
        {
            rb.MovePosition(
                rb.position +
                direction * safeDistance
            );
        }
    }

    private Vector2 CalculateSeparation()
    {
        if (enemyLayer.value == 0 ||
            separationRadius <= 0f)
        {
            return Vector2.zero;
        }

        float searchRadius = Mathf.Max(
            separationRadius,
            bodyRadius * 2f + separationPadding
        );

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                rb.position,
                searchRadius,
                enemyLayer
            );

        HashSet<EnemyBase> checkedEnemies =
            new HashSet<EnemyBase>();

        Vector2 separation = Vector2.zero;

        foreach (Collider2D hit in hits)
        {
            EnemyBase other =
                hit.GetComponentInParent<EnemyBase>();

            if (other == null ||
                other == this ||
                other.IsDead ||
                !checkedEnemies.Add(other))
            {
                continue;
            }

            Vector2 difference =
                rb.position -
                (Vector2)other.transform.position;

            float distance = difference.magnitude;

            if (distance <= 0.001f)
            {
                difference =
                    GetInstanceID() > other.GetInstanceID()
                        ? Vector2.right
                        : Vector2.left;

                distance = 0.001f;
            }

            float influenceRadius = Mathf.Max(
                searchRadius,
                bodyRadius +
                other.BodyRadius +
                separationPadding
            );

            if (distance >= influenceRadius)
                continue;

            float strength =
                1f -
                Mathf.Clamp01(
                    distance / influenceRadius
                );

            separation +=
                difference.normalized *
                strength;
        }

        return separation;
    }

    private void HandleDied()
    {
        if (isDead)
            return;

        isDead = true;

        StopMoving();
        DisablePhysicalBody();
        OnDeathStarted();

        if (deathFeedback != null)
            deathFeedback.PlayDeath();

        OnEnemyDied?.Invoke(this);
        EnemyEvents.RaiseEnemyDied(this);

        DestroyEnemyObject();
    }

    private void DisablePhysicalBody()
    {
        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D bodyCollider in colliders)
            bodyCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    protected virtual void OnDeathStarted()
    {
    }

    protected virtual void DestroyEnemyObject()
    {
        float delay = 0f;

        if (deathFeedback != null)
            delay = deathFeedback.DeathDuration;

        if (deathAnimation != null)
            delay = Mathf.Max(
                delay,
                deathAnimation.Duration
            );

        Destroy(gameObject, delay);
    }
}
