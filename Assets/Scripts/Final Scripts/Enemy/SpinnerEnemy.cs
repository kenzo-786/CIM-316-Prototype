using UnityEngine;
using System.Collections;

public class SpinnerEnemy : EnemyBase
{
    private enum SpinnerState
    {
        Walking,
        Windup,
        Spinning,
        Recovering
    }

    [Header("Spinner")]
    [SerializeField] private float walkDuration = 1.25f;
    [SerializeField] private float windupDuration = 0.45f;
    [SerializeField] private float spinDuration = 1.5f;
    [SerializeField] private float recoverDuration = 0.5f;
    [SerializeField] private float spinSpeedMultiplier = 2.2f;
    [SerializeField] private float spinDamageMultiplier = 1.5f;
    [SerializeField] private float spinHitCooldown = 0.35f;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private SpinnerState state;
    private float stateTimer;
    private float nextSpinHitTime;
    private Vector2 lastTargetDirection = Vector2.down;

    protected override void Awake()
    {
        base.Awake();

        if (telegraph == null)
            telegraph = GetComponent<EnemyTelegraphFeedback>();

        if (animationController == null)
            animationController = GetComponentInChildren<EnemyAnimationController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        animationController?.SetSpinning(false);
        animationController?.SetStationary();

        EnterState(SpinnerState.Walking);
    }

    protected override void TickEnemy()
    {
        stateTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case SpinnerState.Walking:
                TickWalking();
                break;

            case SpinnerState.Windup:
                StopMoving();
                animationController?.SetSpinning(false);
                animationController?.SetStationary();
                animationController?.SetFacingDirection(lastTargetDirection);

                if (stateTimer <= 0f)
                    EnterState(SpinnerState.Spinning);

                break;

            case SpinnerState.Spinning:
                TickSpinning();
                break;

            case SpinnerState.Recovering:
                StopMoving();
                animationController?.SetSpinning(false);
                animationController?.SetStationary();
                animationController?.SetFacingDirection(lastTargetDirection);

                if (stateTimer <= 0f)
                    EnterState(SpinnerState.Walking);

                break;
        }
    }

    private void TickWalking()
    {
        animationController?.SetSpinning(false);

        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                lastTargetDirection = direction.normalized;
                MoveInDirection(direction);
                animationController?.SetMovementDirection(direction);
            }
        }

        if (stateTimer <= 0f)
            EnterState(SpinnerState.Windup);
    }

    private void TickSpinning()
    {
        animationController?.SetSpinning(true);

        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                lastTargetDirection = direction.normalized;
                MoveInDirection(direction, spinSpeedMultiplier);
            }

            if (IsTargetInRange(AttackRange) && Time.time >= nextSpinHitTime)
            {
                nextSpinHitTime = Time.time + spinHitCooldown;

                DamageTarget(
                    ContactDamage * spinDamageMultiplier,
                    target.position
                );
            }
        }

        if (stateTimer <= 0f)
            EnterState(SpinnerState.Recovering);
    }

    private void EnterState(SpinnerState nextState)
    {
        state = nextState;

        switch (state)
        {
            case SpinnerState.Walking:
                stateTimer = walkDuration;
                animationController?.SetSpinning(false);
                break;

            case SpinnerState.Windup:
                stateTimer = windupDuration;
                animationController?.SetSpinning(false);

                if (telegraph != null)
                    telegraph.Begin(windupDuration);

                break;

            case SpinnerState.Spinning:
                stateTimer = spinDuration;
                animationController?.SetSpinning(true);

                if (telegraph != null)
                    telegraph.End();

                break;

            case SpinnerState.Recovering:
                stateTimer = recoverDuration;
                animationController?.SetSpinning(false);

                if (telegraph != null)
                    telegraph.End();

                break;
        }
    }
}
