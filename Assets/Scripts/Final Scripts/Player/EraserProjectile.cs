using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EraserProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float defaultSpeed = 11f;
    [SerializeField] private float defaultLifetime = 4f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private bool rotateToDirection = true;

    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnSpawnedFromPool()
    {
        lifeTimer = 0f;
        damagedTargets.Clear();
    }

    public void OnReturnedToPool()
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
        damagedTargets.Clear();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifetime)
            Despawn();
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
        speed = projectileSpeed > 0f ? projectileSpeed : defaultSpeed;
        lifetime = projectileLifetime > 0f ? projectileLifetime : defaultLifetime;
        pierceLeft = Mathf.Max(0, pierceCount);
        enemyBouncesLeft = Mathf.Max(0, enemyBounceCount);
        wallBouncesLeft = Mathf.Max(0, wallBounceCount);
        owner = projectileOwner;
        lifeTimer = 0f;
        damagedTargets.Clear();

        rb.linearVelocity = direction * speed;

        if (rotateToDirection)
            transform.right = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.gameObject == owner)
            return;

        HandleHit(other, null);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (owner != null && collision.gameObject == owner)
            return;

        HandleHit(collision.collider, collision);
    }

    private void HandleHit(Collider2D other, Collision2D collision)
    {
        if (other == null)
            return;

        if (IsInLayer(other.gameObject.layer, enemyLayer))
        {
            HitEnemy(other);
            return;
        }

        if (IsInLayer(other.gameObject.layer, wallLayer))
            HitWall(other, collision);
    }

    private void HitEnemy(Collider2D enemyCollider)
    {
        IDamageable damageable = enemyCollider.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = enemyCollider.GetComponentInParent<IDamageable>();

        if (damageable == null || damagedTargets.Contains(damageable))
            return;

        damagedTargets.Add(damageable);
        damageable.TakeDamage(new DamageInfo(damage, owner != null ? owner : gameObject, transform.position));

        if (enemyBouncesLeft > 0)
        {
            enemyBouncesLeft--;
            BounceToNearestEnemy();
            return;
        }

        if (pierceLeft > 0)
        {
            pierceLeft--;
            return;
        }

        Despawn();
    }

    private void HitWall(Collider2D wall, Collision2D collision)
    {
        if (wallBouncesLeft <= 0)
        {
            Despawn();
            return;
        }

        wallBouncesLeft--;
        Vector2 normal = GetBounceNormal(wall, collision);

        if (normal == Vector2.zero)
            normal = -direction;

        SetDirection(Vector2.Reflect(direction, normal).normalized);
    }

    private void BounceToNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 8f, enemyLayer);
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
                damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null || damagedTargets.Contains(damageable))
                continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit.transform;
            }
        }

        if (closest == null)
        {
            Despawn();
            return;
        }

        Vector2 nextDirection = ((Vector2)closest.position - (Vector2)transform.position).normalized;
        SetDirection(nextDirection);
    }

    private void SetDirection(Vector2 newDirection)
    {
        direction = newDirection == Vector2.zero ? Vector2.right : newDirection.normalized;
        rb.linearVelocity = direction * speed;

        if (rotateToDirection)
            transform.right = direction;
    }

    private Vector2 GetBounceNormal(Collider2D wall, Collision2D collision)
    {
        if (collision != null && collision.contactCount > 0)
            return collision.GetContact(0).normal;

        Vector2 closestPoint = wall.ClosestPoint(transform.position);
        return ((Vector2)transform.position - closestPoint).normalized;
    }

    private bool IsInLayer(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void Despawn()
    {
        PooledProjectileUtility.Despawn(gameObject);
    }
}
