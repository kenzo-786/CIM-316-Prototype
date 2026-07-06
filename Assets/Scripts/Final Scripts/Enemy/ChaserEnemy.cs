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
            rb.linearVelocity = direction.normalized * enemyData.moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryAttackPlayer();
        }
    }

    private void TryAttackPlayer()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + enemyData.attackCooldown;

        IDamageable damageable = target.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = target.GetComponentInChildren<IDamageable>();

        if (damageable == null)
        {
            Debug.LogWarning("Enemy could not find IDamageable/Health on player target.");
            return;
        }

        Debug.Log("Enemy damaged player.");

        damageable.TakeDamage(new DamageInfo(
            enemyData.contactDamage,
            gameObject,
            target.position
        ));
    }
}
