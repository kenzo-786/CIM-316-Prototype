using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    private float lifeTimer;
    private float currentLifeTime;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float life, float dmg, Vector2 velocity)
    {
        lifeTimer = life;
        damage = dmg;
        currentLifeTime = 0;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.linearVelocity = velocity;
        }
    }

    private void Update()
    {
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTimer)
        {
            BulletPool.Instance.ReturnToPool(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            BulletPool.Instance.ReturnToPool(gameObject);
        }
    }
}
