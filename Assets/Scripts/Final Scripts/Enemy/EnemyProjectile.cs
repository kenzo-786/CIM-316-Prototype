using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private EnemyProjectileData data;
    private int bouncesLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(EnemyProjectileData projectileData, Vector2 direction)
    {
        data = projectileData;
        bouncesLeft = data.wallBounces;

        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * data.speed;
        transform.right = direction.normalized;

        CancelInvoke();
        Invoke(nameof(DestroySelf), data.lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int layer = 1 << other.gameObject.layer;

        if ((data.playerLayer.value & layer) != 0)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(new DamageInfo(
                    data.damage,
                    gameObject,
                    transform.position
                ));
            }

            DestroySelf();
            return;
        }

        if ((data.destroyLayer.value & layer) != 0)
        {
            DestroySelf();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = 1 << collision.gameObject.layer;

        if ((data.wallLayer.value & layer) == 0)
            return;

        if (bouncesLeft <= 0)
        {
            DestroySelf();
            return;
        }

        bouncesLeft--;

        Vector2 normal = collision.contacts[0].normal;
        Vector2 reflected = Vector2.Reflect(rb.linearVelocity.normalized, normal);

        rb.linearVelocity = reflected * data.speed;
        transform.right = reflected;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
