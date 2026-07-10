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

    private SpinnerState state;
    private float stateTimer;
    private float nextSpinHitTime;

    protected override void OnEnable()
    {
        base.OnEnable();
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

                if (stateTimer <= 0f)
                    EnterState(SpinnerState.Spinning);

                break;

            case SpinnerState.Spinning:
                TickSpinning();
                break;

            case SpinnerState.Recovering:
                StopMoving();

                if (stateTimer <= 0f)
                    EnterState(SpinnerState.Walking);

                break;
        }
    }

    private void TickWalking()
    {
        if (target != null)
            MoveToward(target.position);

        if (stateTimer <= 0f)
            EnterState(SpinnerState.Windup);
    }

    private void TickSpinning()
    {
        if (target != null)
        {
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * MoveSpeed * spinSpeedMultiplier * Time.fixedDeltaTime);

            if (IsTargetInRange(AttackRange) && Time.time >= nextSpinHitTime)
            {
                nextSpinHitTime = Time.time + spinHitCooldown;
                DamageTarget(ContactDamage * spinDamageMultiplier, target.position);
            }
        }

        if (stateTimer <= 0f)
            EnterState(SpinnerState.Recovering);
    }

    private void EnterState(SpinnerState nextState)
    {
        state = nextState;

        if (state == SpinnerState.Walking)
            stateTimer = walkDuration;
        else if (state == SpinnerState.Windup)
            stateTimer = windupDuration;
        else if (state == SpinnerState.Spinning)
            stateTimer = spinDuration;
        else
            stateTimer = recoverDuration;
    }
}
