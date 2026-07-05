using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    private float nextAttackTime;

    private void FixedUpdate()
    {
        if (target == null || enemyData == null || IsDead) return;

        Vector2 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance > enemyData.attackRange)
        {
            MoveTowardsPlayer(direction.normalized);
        }
        else
        {
            StopMoving();
            TryAttackPlayer();
        }
    }

    private void MoveTowardsPlayer(Vector2 direction)
    {
        rb.linearVelocity = direction * enemyData.moveSpeed;
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void TryAttackPlayer()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + enemyData.attackCooldown;

        if (target.TryGetComponent(out IDamageable damageable))
        {
            DamageInfo damageInfo = new DamageInfo(
                enemyData.contactDamage,
                gameObject,
                target.position
            );

            damageable.TakeDamage(damageInfo);
        }
    }
}
