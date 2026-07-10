using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    private float nextAttackTime;

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (IsTargetInRange(AttackRange))
        {
            StopMoving();
            TryAttack();
            return;
        }

        MoveToward(target.position);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + AttackCooldown;
        DamageTarget(ContactDamage, target.position);
    }
}
