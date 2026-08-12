using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("Attack Telegraph")]
    [SerializeField, Min(0f)] private float attackWindup = 0.25f;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private float nextAttackTime;
    private float windupTimer;
    private bool windingUp;

    protected override void Awake()
    {
        base.Awake();

        if (telegraph == null)
            telegraph = GetComponent<EnemyTelegraphFeedback>();

        if (animationController == null)
            animationController = GetComponentInChildren<EnemyAnimationController>();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (windingUp)
        {
            StopMoving();
            animationController?.SetStationary();
            FacePlayer();

            windupTimer -= Time.fixedDeltaTime;

            if (windupTimer <= 0f)
                FinishAttack();

            return;
        }

        if (IsTargetInRange(AttackRange))
        {
            StopMoving();
            animationController?.SetStationary();
            FacePlayer();
            TryBeginAttack();
            return;
        }

        Vector2 directionToPlayer = target.position - transform.position;
        animationController?.SetMovementDirection(directionToPlayer);

        MoveToward(target.position);
    }

    private void TryBeginAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        windingUp = true;
        windupTimer = attackWindup;

        animationController?.SetStationary();
        FacePlayer();
        animationController?.PlayAttack();

        if (telegraph != null)
            telegraph.Begin(attackWindup);
    }

    private void FinishAttack()
    {
        windingUp = false;

        if (telegraph != null)
            telegraph.End();

        nextAttackTime = Time.time + AttackCooldown;

        if (IsTargetInRange(AttackRange * 1.15f))
            DamageTarget(ContactDamage, target.position);
    }

    private void FacePlayer()
    {
        if (target == null)
            return;

        animationController?.SetFacingDirection(target.position - transform.position);
    }
}
