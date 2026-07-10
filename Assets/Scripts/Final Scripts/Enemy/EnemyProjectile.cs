using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
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

    private void OnEnable()
    {
        lifeTimer = 0f;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (data != null && lifeTimer >= data.lifetime)
            Destroy(gameObject);
    }

    public void Launch(EnemyProjectileData projectileData, Vector2 launchDirection, float damageMultiplier, GameObject projectileOwner)
    {
        data = projectileData;
        owner = projectileOwner;
        direction = launchDirection.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        damage = data != null ? data.damage * damageMultiplier : 1f;
        bouncesLeft = data != null ? data.wallBounces : 0;
        hitsLeft = data != null ? Mathf.Max(1, data.hitsBeforeDestroy) : 1;

        speed = data != null ? data.speed : 8f;
        rb.velocity = direction * speed;

        if (bodyCollider != null)
            bodyCollider.enabled = true;

        if (data != null && data.rotateToDirection)
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

        if (data != null && IsInLayer(other.gameObject.layer, data.playerLayer))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable == null)
                damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(new DamageInfo(damage, gameObject, transform.position));

            if (data.destroyOnPlayerHit)
            {
                hitsLeft--;

                if (hitsLeft <= 0)
                    Destroy(gameObject);
            }

            return;
        }

        if (data != null && IsInLayer(other.gameObject.layer, data.wallLayer))
        {
            TryBounce(other, collision);
            return;
        }

        if (data != null && IsInLayer(other.gameObject.layer, data.destroyLayer))
            Destroy(gameObject);
    }

    private void TryBounce(Collider2D wall, Collision2D collision)
    {
        if (bouncesLeft <= 0)
        {
            Destroy(gameObject);
            return;
        }

        bouncesLeft--;
        Vector2 normal = GetBounceNormal(wall, collision);

        if (normal == Vector2.zero)
            normal = -direction;

        direction = Vector2.Reflect(direction, normal).normalized;
        rb.velocity = direction * speed;

        if (data != null && data.rotateToDirection)
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
}
