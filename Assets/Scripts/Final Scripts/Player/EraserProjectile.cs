using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EraserProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    private float damage;
    private float speed;
    private float lifetime;
    private GameObject source;
    private LayerMask enemyLayer;
    private LayerMask wallBounceLayer;
    private LayerMask destroyLayer;

    private int pierceLeft;
    private int enemyBouncesLeft;
    private int wallBouncesLeft;

    private readonly HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        float damage,
        float speed,
        float lifetime,
        Vector2 direction,
        GameObject source,
        LayerMask enemyLayer,
        LayerMask wallBounceLayer,
        LayerMask destroyLayer,
        int pierceCount,
        int enemyBounceCount,
        int wallBounceCount)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.source = source;
        this.enemyLayer = enemyLayer;
        this.wallBounceLayer = wallBounceLayer;
        this.destroyLayer = destroyLayer;
        this.pierceLeft = pierceCount;
        this.enemyBouncesLeft = enemyBounceCount;
        this.wallBouncesLeft = wallBounceCount;

        alreadyHit.Clear();

        rb.linearVelocity = direction.normalized * speed;
        transform.right = direction.normalized;

        CancelInvoke();
        Invoke(nameof(DestroySelf), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = 1 << other.gameObject.layer;

        if ((enemyLayer.value & otherLayer) != 0)
        {
            HitEnemy(other);
            return;
        }

        if ((destroyLayer.value & otherLayer) != 0)
        {
            DestroySelf();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int otherLayer = 1 << collision.gameObject.layer;

        if ((wallBounceLayer.value & otherLayer) == 0)
            return;

        if (wallBouncesLeft <= 0)
        {
            DestroySelf();
            return;
        }

        wallBouncesLeft--;

        Vector2 normal = collision.contacts[0].normal;
        Vector2 reflected = Vector2.Reflect(rb.linearVelocity.normalized, normal);

        rb.linearVelocity = reflected * speed;
        transform.right = reflected;
    }

    private void HitEnemy(Collider2D enemyCollider)
    {
        if (alreadyHit.Contains(enemyCollider))
            return;

        alreadyHit.Add(enemyCollider);

        if (enemyCollider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(
                new DamageInfo(damage, source, enemyCollider.transform.position)
            );
        }

        if (enemyBouncesLeft > 0)
        {
            enemyBouncesLeft--;
            BounceToNearestEnemy(enemyCollider.transform);
            return;
        }

        if (pierceLeft > 0)
        {
            pierceLeft--;
            return;
        }

        DestroySelf();
    }

    private void BounceToNearestEnemy(Transform currentEnemy)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            8f,
            enemyLayer
        );

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.transform == currentEnemy) continue;
            if (alreadyHit.Contains(hit)) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = hit.transform;
            }
        }

        if (nearest == null)
        {
            DestroySelf();
            return;
        }

        Vector2 direction = nearest.position - transform.position;
        rb.linearVelocity = direction.normalized * speed;
        transform.right = direction.normalized;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
