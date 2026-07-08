using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class MageEnemy : EnemyBase
{
    [Header("Mage")]
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float retreatRange = 3f;
    [SerializeField] private float shootCooldown = 1.8f;
    [SerializeField] private int shotsPerCast = 3;
    [SerializeField] private float spreadAngle = 12f;

    private EnemyProjectileShooter shooter;
    private float nextShootTime;

    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<EnemyProjectileShooter>();
    }

    private void FixedUpdate()
    {
        if (target == null || enemyData == null || IsDead) return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance < retreatRange)
        {
            RetreatFromPlayer();
        }
        else if (distance > preferredRange)
        {
            MoveTowardPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryCast();
        }
    }

    private void MoveTowardPlayer()
    {
        Vector2 direction = target.position - transform.position;
        rb.linearVelocity = direction.normalized * enemyData.moveSpeed;
    }

    private void RetreatFromPlayer()
    {
        Vector2 direction = transform.position - target.position;
        rb.linearVelocity = direction.normalized * enemyData.moveSpeed;
    }

    private void TryCast()
    {
        if (Time.time < nextShootTime) return;

        nextShootTime = Time.time + shootCooldown;

        Vector2 baseDirection = target.position - transform.position;

        if (shotsPerCast <= 1)
        {
            shooter.ShootAt(target.position);
            return;
        }

        int middle = shotsPerCast / 2;

        for (int i = 0; i < shotsPerCast; i++)
        {
            float angleOffset = (i - middle) * spreadAngle;
            Vector2 direction = Rotate(baseDirection.normalized, angleOffset);
            Vector3 fakeTarget = transform.position + (Vector3)(direction * 10f);

            shooter.ShootAt(fakeTarget);
        }
    }

    private Vector2 Rotate(Vector2 direction, float angle)
    {
        return Quaternion.Euler(0f, 0f, angle) * direction;
    }

}
