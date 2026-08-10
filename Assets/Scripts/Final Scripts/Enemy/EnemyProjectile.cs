using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour, IPoolable
{
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private EnemyProjectileData data;
    private GameObject owner;
    private float damage;
    private int bouncesLeft;
    private int hitsLeft;
    private float lifeTimer;
    private Vector2 direction;
    private float speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    public void OnSpawnedFromPool()
    {
        lifeTimer = 0f;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }
    }

    public void OnReturnedToPool()
    {
        data = null;
        owner = null;
        damage = 0f;
        bouncesLeft = 0;
        hitsLeft = 0;
        lifeTimer = 0f;
        direction = Vector2.zero;
        speed = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (data != null && lifeTimer >= data.lifetime)
        {
            Despawn();
        }
    }

    public void Launch(
        EnemyProjectileData projectileData,
        Vector2 launchDirection,
        float damageMultiplier,
        GameObject projectileOwner)
    {
        data = projectileData;
        owner = projectileOwner;
        direction = launchDirection.normalized;

        if (direction == Vector2.zero)
        {
            direction = Vector2.right;
        }

        damage = data != null
            ? data.damage * damageMultiplier
            : 1f;

        bouncesLeft = data != null
            ? data.wallBounces
            : 0;

        hitsLeft = data != null
            ? Mathf.Max(1, data.hitsBeforeDestroy)
            : 1;

        speed = data != null
            ? data.speed
            : 8f;

        rb.linearVelocity = direction * speed;

        if (data != null && data.rotateToDirection)
        {
            transform.right = direction;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.gameObject == owner)
        {
            return;
        }

        HandleHit(other, null);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (owner != null && collision.gameObject == owner)
        {
            return;
        }

        HandleHit(collision.collider, collision);
    }

    private void HandleHit(
        Collider2D other,
        Collision2D collision)
    {
        if (other == null || data == null)
        {
            return;
        }

        if (IsInLayer(other.gameObject.layer, data.playerLayer))
        {
            IDamageable damageable =
                other.GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable =
                    other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                GameObject damageSource =
                    owner != null
                        ? owner
                        : gameObject;

                Vector2 hitPoint =
                    other.ClosestPoint(transform.position);

                damageable.TakeDamage(
                    new DamageInfo(
                        damage,
                        damageSource,
                        hitPoint
                    )
                );
            }

            if (data.destroyOnPlayerHit)
            {
                hitsLeft--;

                if (hitsLeft <= 0)
                {
                    Despawn();
                }
            }

            return;
        }

        if (IsInLayer(other.gameObject.layer, data.wallLayer))
        {
            TryBounce(other, collision);
            return;
        }

        if (IsInLayer(other.gameObject.layer, data.destroyLayer))
        {
            Despawn();
        }
    }

    private void TryBounce(
        Collider2D wall,
        Collision2D collision)
    {
        if (bouncesLeft <= 0)
        {
            Despawn();
            return;
        }

        bouncesLeft--;

        Vector2 normal =
            GetBounceNormal(wall, collision);

        if (normal == Vector2.zero)
        {
            normal = -direction;
        }

        direction =
            Vector2.Reflect(direction, normal).normalized;

        rb.linearVelocity = direction * speed;

        if (data != null && data.rotateToDirection)
        {
            transform.right = direction;
        }
    }

    private Vector2 GetBounceNormal(
        Collider2D wall,
        Collision2D collision)
    {
        if (collision != null &&
            collision.contactCount > 0)
        {
            return collision.GetContact(0).normal;
        }

        Vector2 closestPoint =
            wall.ClosestPoint(transform.position);

        return
            ((Vector2)transform.position - closestPoint)
            .normalized;
    }

    private bool IsInLayer(
        int layer,
        LayerMask mask)
    {
        return
            (mask.value & (1 << layer)) != 0;
    }

    private void Despawn()
    {
        PooledProjectileUtility.Despawn(gameObject);
    }
}
