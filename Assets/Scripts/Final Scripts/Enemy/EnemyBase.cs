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
    [SerializeField, Min(0f)] private float obstacleProbeDistance = 1.1f;
    [SerializeField, Min(0f)] private float obstacleProbeRadius = 0.22f;
    [SerializeField, Min(0f)] private float wallFollowStrength = 1.35f;
    [SerializeField, Min(0f)] private float wallFollowMemory = 0.65f;
    [SerializeField, Min(0f)] private float wallClearanceProbeDistance = 1.15f;

    protected Rigidbody2D rb;
    protected Health health;
    protected Transform target;

    private bool isDead;
    private EnemyDeathFeedback deathFeedback;
    private CodeDeathAnimation deathAnimation;
    private Collider2D bodyCollider;
    private RoomDifficultySnapshot currentDifficulty = RoomDifficultySnapshot.Default;
    private float collisionRadius = 0.4f;
    private Vector2 wallFollowDirection;
    private float wallFollowUntil;

    public event Action<EnemyBase> OnEnemyDied;

    public EnemyData EnemyData => enemyData;
    public Transform Target => target;
    public RoomDifficultySnapshot CurrentDifficulty => currentDifficulty;
    public bool IsDead => isDead;
    public float BodyRadius => collisionRadius;

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
        bodyCollider = GetComponent<Collider2D>();

        if (deathFeedback == null)
            deathFeedback = gameObject.AddComponent<EnemyDeathFeedback>();

        if (visual == null)
            visual = GetComponentInChildren<SpriteRenderer>();

        if (bodyCollider == null)
            bodyCollider = GetComponentInChildren<Collider2D>();

        if (enemyLayer.value == 0)
            enemyLayer = LayerMask.GetMask("Enemy");

        if (obstacleLayer.value == 0)
            obstacleLayer = LayerMask.GetMask("Wall", "Door", "Obstacle");

        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            collisionRadius = Mathf.Max(bounds.extents.x, bounds.extents.y);
        }

        collisionRadius = Mathf.Max(collisionRadius, 0.2f);
    }

    protected virtual void OnEnable()
    {
        isDead = false;
        wallFollowUntil = 0f;

        if (health != null)
            health.OnDied += HandleDied;
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    public virtual void Initialize(EnemyData data, Transform playerTarget)
    {
        enemyData = data;
        target = playerTarget;
        isDead = false;
        wallFollowUntil = 0f;

        if (enemyData != null && health != null)
            health.SetMaxHealth(enemyData.maxHealth, true);
    }

    public virtual void ApplyDifficulty(RoomDifficultySnapshot difficulty)
    {
        currentDifficulty = difficulty;

        if (enemyData != null && health != null)
        {
            health.SetMaxHealth(
                enemyData.maxHealth * currentDifficulty.healthMultiplier,
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

    protected void MoveInDirection(Vector2 direction, float speedMultiplier = 1f)
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

        float moveDistance = MoveSpeed * speedMultiplier * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + steeringDirection * moveDistance);
        FaceDirection(steeringDirection);
    }

    protected bool HasClearPathTo(Vector2 worldPosition)
    {
        if (obstacleLayer.value == 0)
            return true;

        Vector2 direction = worldPosition - rb.position;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return true;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            direction.normalized,
            distance,
            obstacleLayer
        );

        return hit.collider == null;
    }

    protected Vector2 GetSteeredDirection(Vector2 desiredDirection)
    {
        Vector2 desired = desiredDirection.normalized;

        if (desired == Vector2.zero)
            return Vector2.zero;

        Vector2 separation = CalculateSeparation();
        Vector2 normalSteering = desired + separation * separationStrength;

        if (normalSteering.sqrMagnitude <= 0.0001f)
            normalSteering = desired;

        normalSteering.Normalize();

        if (obstacleLayer.value == 0 || obstacleProbeDistance <= 0f)
            return normalSteering;

        float probeRadius = Mathf.Clamp(
            Mathf.Min(obstacleProbeRadius, collisionRadius * 0.75f),
            0.08f,
            0.45f
        );

        RaycastHit2D obstacleHit = Physics2D.CircleCast(
            rb.position,
            probeRadius,
            normalSteering,
            obstacleProbeDistance,
            obstacleLayer
        );

        if (obstacleHit.collider == null)
        {
            wallFollowUntil = 0f;
            return normalSteering;
        }

        if (Time.time < wallFollowUntil)
        {
            return BuildWallFollowDirection(
                desired,
                separation,
                wallFollowDirection
            );
        }

        Vector2 wallNormal = obstacleHit.normal;

        if (wallNormal.sqrMagnitude <= 0.0001f)
            wallNormal = -normalSteering;

        Vector2 firstSide = new Vector2(
            -wallNormal.y,
            wallNormal.x
        ).normalized;

        Vector2 secondSide = -firstSide;

        Vector2 firstCandidate = (
            firstSide + wallNormal * 0.35f
        ).normalized;

        Vector2 secondCandidate = (
            secondSide + wallNormal * 0.35f
        ).normalized;

        float firstScore = ScoreWallFollowDirection(
            firstCandidate,
            desired
        );

        float secondScore = ScoreWallFollowDirection(
            secondCandidate,
            desired
        );

        if (Mathf.Approximately(firstScore, secondScore))
        {
            wallFollowDirection = GetInstanceID() % 2 == 0
                ? firstCandidate
                : secondCandidate;
        }
        else
        {
            wallFollowDirection = firstScore > secondScore
                ? firstCandidate
                : secondCandidate;
        }

        wallFollowUntil = Time.time + wallFollowMemory;

        return BuildWallFollowDirection(
            desired,
            separation,
            wallFollowDirection
        );
    }

    protected void StopMoving()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    protected bool IsTargetInRange(float range)
    {
        return target != null &&
               Vector2.Distance(transform.position, target.position) <= range;
    }

    protected void DamageTarget(float damage, Vector2 hitPoint)
    {
        if (target == null || damage <= 0f)
            return;

        IDamageable damageable = target.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(
                new DamageInfo(damage, gameObject, hitPoint)
            );
        }
    }

    protected IDamageable GetDamageable(Collider2D hit)
    {
        if (hit == null)
            return null;

        IDamageable damageable = hit.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = hit.GetComponentInParent<IDamageable>();

        return damageable;
    }

    protected void FaceDirection(Vector2 direction)
    {
        if (visual == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        visual.flipX = direction.x < 0f;
    }

    private Vector2 BuildWallFollowDirection(
        Vector2 desired,
        Vector2 separation,
        Vector2 followDirection
    )
    {
        Vector2 result =
            followDirection * wallFollowStrength +
            desired * 0.05f +
            separation * separationStrength;

        return result.sqrMagnitude > 0.0001f
            ? result.normalized
            : followDirection;
    }

    private float ScoreWallFollowDirection(
        Vector2 candidateDirection,
        Vector2 desiredDirection
    )
    {
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            candidateDirection,
            wallClearanceProbeDistance,
            obstacleLayer
        );

        float clearance = hit.collider == null
            ? wallClearanceProbeDistance
            : hit.distance;

        float desiredAlignment = Mathf.Max(
            0f,
            Vector2.Dot(candidateDirection, desiredDirection)
        );

        return clearance + desiredAlignment * 0.35f;
    }

    private Vector2 CalculateSeparation()
    {
        if (enemyLayer.value == 0 || separationRadius <= 0f)
            return Vector2.zero;

        float searchRadius = Mathf.Max(
            separationRadius,
            collisionRadius * 2f + separationPadding
        );

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            rb.position,
            searchRadius,
            enemyLayer
        );

        HashSet<EnemyBase> checkedEnemies = new HashSet<EnemyBase>();
        Vector2 separation = Vector2.zero;

        foreach (Collider2D hit in hits)
        {
            EnemyBase other = hit.GetComponentInParent<EnemyBase>();

            if (other == null ||
                other == this ||
                other.IsDead ||
                !checkedEnemies.Add(other))
            {
                continue;
            }

            Vector2 difference = rb.position - (Vector2)other.transform.position;
            float distance = difference.magnitude;

            if (distance <= 0.001f)
            {
                difference = GetInstanceID() > other.GetInstanceID()
                    ? Vector2.right
                    : Vector2.left;

                distance = 0.001f;
            }

            float influenceRadius = Mathf.Max(
                searchRadius,
                collisionRadius + other.BodyRadius + separationPadding
            );

            if (distance >= influenceRadius)
                continue;

            float strength = 1f - Mathf.Clamp01(
                distance / influenceRadius
            );

            separation += difference.normalized * strength;
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
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D collider in colliders)
            collider.enabled = false;

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
            delay = Mathf.Max(delay, deathAnimation.Duration);

        Destroy(gameObject, delay);
    }
}
