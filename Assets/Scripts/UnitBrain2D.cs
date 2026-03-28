using UnityEngine;

public class UnitBrain2D : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData data;

    [Header("Stats")]
    public string targetTag;

    //keep these values incase no SO is assigned to the enemy
    public float moveSpeed = 5f;
    public float health = 30f;
    public float damage = 10f;

    protected Rigidbody2D rb;   //changed to protected for inheritance
    private float lastAttackTime;
    public float attackCooldown = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (data != null)
        {
            health = data.health;
            damage = data.damage;
            moveSpeed = data.moveSpeed;
        }
    }

    void FixedUpdate()
    {
        Transform target = GetClosestEnemy();

        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;

            rb.AddForce(direction * moveSpeed);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    //changed to protected for child classes
    protected Transform GetClosestEnemy()
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

                    Vector2 knockback = (transform.position - collision.transform.position).normalized;
                    rb.AddForce(knockback * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        transform.localScale *= 0.9f;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}