using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EraserProjectile : MonoBehaviour, IPoolable
{
    [Header("Movement")]
    [SerializeField] private float defaultSpeed = 11f;
    [SerializeField] private float defaultLifetime = 4f;
    [SerializeField] private bool rotateToDirection = true;

    [Header("Collision")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Homing")]
    [SerializeField] private float defaultHomingTurnSpeed = 360f;
    [SerializeField] private float bounceSearchRadius = 8f;

    private readonly HashSet<IDamageable>
        damagedTargets =
            new HashSet<IDamageable>();

    private Rigidbody2D rb;
    private float damage;
    private float speed;
    private float lifetime;
    private float lifeTimer;

    private int pierceLeft;
    private int enemyBouncesLeft;
    private int wallBouncesLeft;

    private Vector2 direction;
    private GameObject owner;

    private bool homingEnabled;
    private Transform homingTarget;
    private float homingTurnSpeed;
    private bool despawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;
    }

    public void OnSpawnedFromPool()
    {
        ResetRuntimeState();
    }

    public void OnReturnedToPool()
    {
        ResetRuntimeState();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (lifetime <= 0f || despawning)
            return;

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifetime)
            Despawn();
    }

    private void FixedUpdate()
    {
        UpdateHoming();
    }

    public void Launch(
        Vector2 launchDirection,
        float projectileDamage,
        float projectileSpeed,
        float projectileLifetime,
        int pierceCount,
        int enemyBounceCount,
        int wallBounceCount,
        GameObject projectileOwner)
    {
        direction = launchDirection.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        damage = projectileDamage;

        speed = projectileSpeed > 0f
            ? projectileSpeed
            : defaultSpeed;

        lifetime = projectileLifetime > 0f
            ? projectileLifetime
            : defaultLifetime;

        pierceLeft = Mathf.Max(0, pierceCount);

        enemyBouncesLeft =
            Mathf.Max(0, enemyBounceCount);

        wallBouncesLeft =
            Mathf.Max(0, wallBounceCount);

        owner = projectileOwner;
        lifeTimer = 0f;
        despawning = false;

        homingEnabled = false;
        homingTarget = null;
        homingTurnSpeed =
            defaultHomingTurnSpeed;

        damagedTargets.Clear();

        ApplyDirection();
    }

    public void SetHomingTarget(
        Transform target,
        float turnSpeed)
    {
        homingTarget = target;
        homingEnabled = target != null;

        homingTurnSpeed = turnSpeed > 0f
            ? turnSpeed
            : defaultHomingTurnSpeed;
    }

    private void UpdateHoming()
    {
        if (!homingEnabled ||
            homingTarget == null ||
            rb == null ||
            despawning)
        {
            return;
        }

        if (!IsHomingTargetValid(homingTarget))
        {
            // Continue travelling forward instead of
            // suddenly selecting a different target.
            homingTarget = null;
            homingEnabled = false;
            return;
        }

        Vector2 desiredDirection =
            ((Vector2)homingTarget.position -
             rb.position).normalized;

        if (desiredDirection == Vector2.zero)
            return;

        float signedAngle =
            Vector2.SignedAngle(
                direction,
                desiredDirection
            );

        float maximumTurn =
            homingTurnSpeed *
            Time.fixedDeltaTime;

        float appliedTurn =
            Mathf.Clamp(
                signedAngle,
                -maximumTurn,
                maximumTurn
            );

        Vector3 rotated =
            Quaternion.Euler(
                0f,
                0f,
                appliedTurn
            ) * direction;

        direction = new Vector2(
            rotated.x,
            rotated.y
        ).normalized;

        ApplyDirection();
    }

    private bool IsHomingTargetValid(
        Transform target)
    {
        if (target == null ||
            !target.gameObject.activeInHierarchy)
        {
            return false;
        }

        EnemyBase enemy =
            target.GetComponentInParent<EnemyBase>();

        IDamageable damageable =
            target.GetComponentInParent<IDamageable>();

        if (enemy == null ||
            enemy.IsDead ||
            damageable == null)
        {
            return false;
        }

        return !damagedTargets.Contains(damageable);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (despawning || IsOwner(other))
            return;

        HandleHit(other, null);
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (despawning ||
            IsOwner(collision.collider))
        {
            return;
        }

        HandleHit(
            collision.collider,
            collision
        );
    }

    private void HandleHit(
        Collider2D other,
        Collision2D collision)
    {
        if (other == null)
            return;

        if (IsInLayer(
            other.gameObject.layer,
            enemyLayer))
        {
            HitEnemy(other);
            return;
        }

        if (IsInLayer(
            other.gameObject.layer,
            wallLayer))
        {
            HitWall(other, collision);
        }
    }

    private void HitEnemy(
        Collider2D enemyCollider)
    {
        IDamageable damageable =
            enemyCollider
                .GetComponentInParent<IDamageable>();

        if (damageable == null ||
            damagedTargets.Contains(damageable))
        {
            return;
        }

        damagedTargets.Add(damageable);

        damageable.TakeDamage(
            new DamageInfo(
                damage,
                owner != null
                    ? owner
                    : gameObject,
                transform.position
            )
        );

        if (enemyBouncesLeft > 0)
        {
            enemyBouncesLeft--;
            BounceToNearestEnemy();
            return;
        }

        if (pierceLeft > 0)
        {
            pierceLeft--;

            homingTarget = null;
            homingEnabled = false;

            return;
        }

        Despawn();
    }

    private void HitWall(
        Collider2D wall,
        Collision2D collision)
    {
        if (wallBouncesLeft <= 0)
        {
            Despawn();
            return;
        }

        wallBouncesLeft--;

        Vector2 normal =
            GetBounceNormal(wall, collision);

        if (normal == Vector2.zero)
            normal = -direction;

        SetDirection(
            Vector2.Reflect(
                direction,
                normal
            ).normalized
        );
    }

    private void BounceToNearestEnemy()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                bounceSearchRadius,
                enemyLayer
            );

        EnemyBase closestEnemy = null;
        float closestDistanceSquared =
            float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy =
                hit.GetComponentInParent<EnemyBase>();

            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (enemy == null ||
                enemy.IsDead ||
                damageable == null ||
                damagedTargets.Contains(damageable))
            {
                continue;
            }

            RaycastHit2D obstruction =
                Physics2D.Linecast(
                    transform.position,
                    enemy.transform.position,
                    wallLayer
                );

            if (obstruction.collider != null)
                continue;

            float distanceSquared =
                ((Vector2)enemy.transform.position -
                 (Vector2)transform.position)
                .sqrMagnitude;

            if (distanceSquared >=
                closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared =
                distanceSquared;

            closestEnemy = enemy;
        }

        if (closestEnemy == null)
        {
            Despawn();
            return;
        }

        homingTarget = closestEnemy.transform;
        homingEnabled = true;

        Vector2 nextDirection =
            ((Vector2)homingTarget.position -
             (Vector2)transform.position)
            .normalized;

        SetDirection(nextDirection);
    }

    private void SetDirection(
        Vector2 newDirection)
    {
        direction = newDirection == Vector2.zero
            ? Vector2.right
            : newDirection.normalized;

        ApplyDirection();
    }

    private void ApplyDirection()
    {
        if (rb != null)
            rb.linearVelocity = direction * speed;

        if (rotateToDirection)
            transform.right = direction;
    }

    private Vector2 GetBounceNormal(
        Collider2D wall,
        Collision2D collision)
    {
        if (collision != null &&
            collision.contactCount > 0)
        {
            return collision
                .GetContact(0)
                .normal;
        }

        Vector2 closestPoint =
            wall.ClosestPoint(transform.position);

        return
            ((Vector2)transform.position -
             closestPoint).normalized;
    }

    private bool IsOwner(Collider2D other)
    {
        if (owner == null || other == null)
            return false;

        return
            other.transform.root ==
            owner.transform.root;
    }

    private bool IsInLayer(
        int layer,
        LayerMask mask)
    {
        return
            (mask.value & (1 << layer)) != 0;
    }

    private void ResetRuntimeState()
    {
        damage = 0f;
        speed = 0f;
        lifetime = 0f;
        lifeTimer = 0f;

        pierceLeft = 0;
        enemyBouncesLeft = 0;
        wallBouncesLeft = 0;

        direction = Vector2.zero;
        owner = null;

        homingEnabled = false;
        homingTarget = null;

        homingTurnSpeed =
            defaultHomingTurnSpeed;

        despawning = false;
        damagedTargets.Clear();
    }

    private void Despawn()
    {
        if (despawning)
            return;

        despawning = true;

        PooledProjectileUtility.Despawn(
            gameObject
        );
    }
}
