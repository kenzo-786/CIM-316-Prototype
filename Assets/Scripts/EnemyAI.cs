using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public EnemyData data;
    public GameObject expPrefab;

    private float currentHealth;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isDashing = false;
    private float nextDashTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        if (data != null) currentHealth = data.maxHealth;
        if (sr != null) originalColor = sr.color;
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    void FixedUpdate()
    {
        if (player == null || data == null || isDashing) return;

        switch (data.behaviorType)
        {
            case EnemyBehavior.Chaser:
            case EnemyBehavior.Tank:
            case EnemyBehavior.Swarmer:
                HandleStandardMovement();
                break;

            case EnemyBehavior.Dasher:
                HandleDasherLogic();
                break;
        }
    }

    void HandleStandardMovement()
    {
        Vector2 dir = (Vector2)player.position - rb.position;
        rb.linearVelocity = dir.normalized * data.moveSpeed;
        RotateTowards(dir);
    }

    void HandleDasherLogic()
    {
        Vector2 dir = (Vector2)player.position - rb.position;

        if (Time.time >= nextDashTime)
        {
            StartCoroutine(PerformDash(dir.normalized));
        }
        else
        {
            rb.linearVelocity = dir.normalized * (data.moveSpeed * 0.5f);
            RotateTowards(dir);
        }
    }

    IEnumerator PerformDash(Vector2 dashDir)
    {
        isDashing = true;
        nextDashTime = Time.time + data.dashCooldown;

        rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = data.telegraphColor;
        yield return new WaitForSeconds(0.5f);

        if (sr != null) sr.color = originalColor;
        rb.linearVelocity = dashDir * data.dashSpeed;

        yield return new WaitForSeconds(data.dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (expPrefab != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0);
            Instantiate(expPrefab, spawnPos, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null) pc.TakeDamage(data.damage * Time.deltaTime);
        }
    }
}
