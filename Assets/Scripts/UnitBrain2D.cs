using UnityEngine;

public class UnitBrain2D : MonoBehaviour
{
    [Header("Stats")]
    public string targetTag;
    public float moveSpeed = 5f;
    public float health = 30f;
    public float damage = 10f;

    private Rigidbody2D rb;
    private float lastAttackTime;
    public float attackCooldown = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Transform target = GetClosestEnemy();

        if (target != null)
        {
            // Calculate direction in 2D
            Vector2 direction = (target.position - transform.position).normalized;

            // Push the unit towards the enemy
            rb.AddForce(direction * moveSpeed);

            // Optional: Rotate to face the enemy (Z-axis rotation)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    Transform GetClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
        }
        return bestTarget;
    }

    // 2D Collision Logic
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            if (Time.time > lastAttackTime + attackCooldown)
            {
                UnitBrain2D enemyScript = collision.gameObject.GetComponent<UnitBrain2D>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(damage);
                    lastAttackTime = Time.time;

                    // Simple Bounce Back
                    Vector2 knockback = (transform.position - collision.transform.position).normalized;
                    rb.AddForce(knockback * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        transform.localScale *= 0.9f; // Visual feedback

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
