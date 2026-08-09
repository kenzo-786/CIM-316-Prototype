using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("Attack Telegraph")]
    [SerializeField, Min(0f)] private float attackWindup = 0.25f;
    [SerializeField] private EnemyTelegraphFeedback telegraph;

    private float nextAttackTime;
    private float windupTimer;
    private bool windingUp;

    protected override void Awake()
    {
        base.Awake();

        if (telegraph == null)
        {
            telegraph = GetComponent<EnemyTelegraphFeedback>();
        }
    }

    protected override void TickEnemy()
    {
        if (target == null)
        {
            return;
        }

        if (windingUp)
        {
            StopMoving();
            windupTimer -= Time.fixedDeltaTime;

            if (windupTimer <= 0f)
            {
                FinishAttack();
            }

            return;
        }

        if (IsTargetInRange(AttackRange))
        {
            StopMoving();
            TryBeginAttack();
            return;
        }

        MoveToward(target.position);
    }

    private void TryBeginAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        windingUp = true;
        windupTimer = attackWindup;

        if (telegraph != null)
        {
            telegraph.Begin(attackWindup);
        }
    }

    private void FinishAttack()
    {
        windingUp = false;

        if (telegraph != null)
        {
            telegraph.End();
        }

        nextAttackTime = Time.time + AttackCooldown;

        if (IsTargetInRange(AttackRange * 1.15f))
        {
            DamageTarget(ContactDamage, target.position);
        }
    }
}
