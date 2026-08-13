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

    [Header("Cycle Timing")]
    [SerializeField] private float normalMoveDurationMin = 2f;
    [SerializeField] private float normalMoveDurationMax = 3.25f;
    [SerializeField] private float windupDuration = 1.33f;
    [SerializeField] private float spinDuration = 1.4f;
    [SerializeField] private float recoverDuration = 0.85f;
    [SerializeField] private float spinCooldown = 4f;

    [Header("Spin Combat")]
    [SerializeField] private float spinSpeedMultiplier = 2.15f;
    [SerializeField] private float spinDamageMultiplier = 1.3f;
    [SerializeField] private float spinHitCooldown = 0.45f;
    [SerializeField] private float spinHitStartDelay = 0.1f;
    [SerializeField] private float minimumSpinStartDistance = 1.4f;
    [SerializeField] private float maximumSpinStartDistance = 6f;

    [Header("Movement")]
    [SerializeField] private float preferredDistance = 3.5f;
    [SerializeField] private float orbitStrength = 0.7f;
    [SerializeField] private float recoveryRetreatSpeedMultiplier = 0.8f;

    [Header("References")]
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private SpinnerState state;
    private float stateTimer;
    private float nextSpinAllowedTime;
    private float nextSpinHitTime;
    private Vector2 lastTargetDirection = Vector2.down;
    private float orbitDirection = 1f;

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

        nextSpinAllowedTime = Time.time + 1f;
        nextSpinHitTime = 0f;
        orbitDirection = Random.value < 0.5f ? -1f : 1f;

        animationController?.SetSpinning(false);
        animationController?.SetWindingUp(false);
        animationController?.SetRecovering(false);
        animationController?.SetStationary();

        EnterState(SpinnerState.Walking);
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        stateTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case SpinnerState.Walking:
                TickWalking();
                break;

            case SpinnerState.Windup:
                TickWindup();
                break;

            case SpinnerState.Spinning:
                TickSpinning();
                break;

            case SpinnerState.Recovering:
                TickRecovering();
                break;
        }
    }

    private void TickWalking()
    {
        animationController?.SetSpinning(false);
        animationController?.SetWindingUp(false);
        animationController?.SetRecovering(false);

        Vector2 toPlayer = (Vector2)target.position - rb.position;

        if (toPlayer.sqrMagnitude > 0.0001f)
            lastTargetDirection = toPlayer.normalized;

        float distanceToPlayer = toPlayer.magnitude;
        bool hasClearPath = HasClearPathTo(target.position);

        Vector2 movementDirection = hasClearPath
            ? GetWalkingDirection(toPlayer, distanceToPlayer)
            : toPlayer.normalized;

        MoveInDirection(movementDirection);
        animationController?.SetMovementDirection(movementDirection);

        if (!hasClearPath)
        {
            stateTimer = Mathf.Max(stateTimer, 0.25f);
            return;
        }

        if (stateTimer > 0f || !CanStartSpin(distanceToPlayer))
            return;

        EnterState(SpinnerState.Windup);
    }

    private void TickWindup()
    {
        StopMoving();
        animationController?.SetStationary();
        animationController?.SetSpinning(false);
        animationController?.SetWindingUp(true);
        animationController?.SetRecovering(false);
        animationController?.SetFacingDirection(lastTargetDirection);

        if (stateTimer > 0f)
            return;

        float distanceToPlayer = Vector2.Distance(
            rb.position,
            target.position
        );

        if (!HasClearPathTo(target.position) ||
            distanceToPlayer > maximumSpinStartDistance * 1.25f)
        {
            EnterState(SpinnerState.Walking);
            return;
        }

        EnterState(SpinnerState.Spinning);
    }

    private void TickSpinning()
    {
        animationController?.SetSpinning(true);
        animationController?.SetWindingUp(false);
        animationController?.SetRecovering(false);

        if (!HasClearPathTo(target.position))
        {
            EnterState(SpinnerState.Recovering);
            return;
        }

        Vector2 directionToPlayer = (Vector2)target.position - rb.position;

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            lastTargetDirection = directionToPlayer.normalized;

            MoveInDirection(
                directionToPlayer,
                spinSpeedMultiplier
            );

            animationController?.SetMovementDirection(directionToPlayer);
        }

        if (IsTargetInRange(AttackRange) &&
            Time.time >= nextSpinHitTime)
        {
            nextSpinHitTime = Time.time + spinHitCooldown;

            DamageTarget(
                ContactDamage * spinDamageMultiplier,
                target.position
            );
        }

        if (stateTimer <= 0f)
            EnterState(SpinnerState.Recovering);
    }

    private void TickRecovering()
    {
        animationController?.SetSpinning(false);
        animationController?.SetWindingUp(false);
        animationController?.SetRecovering(true);

        Vector2 retreatDirection = -lastTargetDirection;

        MoveInDirection(
            retreatDirection,
            recoveryRetreatSpeedMultiplier
        );

        animationController?.SetMovementDirection(retreatDirection);

        if (stateTimer <= 0f)
            EnterState(SpinnerState.Walking);
    }

    private Vector2 GetWalkingDirection(
        Vector2 toPlayer,
        float distanceToPlayer
    )
    {
        if (distanceToPlayer <= 0.0001f)
            return Vector2.zero;

        if (distanceToPlayer < preferredDistance * 0.65f)
            return -toPlayer.normalized;

        if (distanceToPlayer > preferredDistance * 1.2f)
            return toPlayer.normalized;

        Vector2 sideDirection = new Vector2(
            -toPlayer.y,
            toPlayer.x
        ).normalized * orbitDirection;

        return (
            toPlayer.normalized * 0.25f +
            sideDirection * orbitStrength
        ).normalized;
    }

    private bool CanStartSpin(float distanceToPlayer)
    {
        return Time.time >= nextSpinAllowedTime &&
               distanceToPlayer >= minimumSpinStartDistance &&
               distanceToPlayer <= maximumSpinStartDistance;
    }

    private void EnterState(SpinnerState nextState)
    {
        state = nextState;

        switch (state)
        {
            case SpinnerState.Walking:
                stateTimer = Random.Range(
                    normalMoveDurationMin,
                    normalMoveDurationMax
                );

                orbitDirection = Random.value < 0.5f ? -1f : 1f;

                animationController?.SetSpinning(false);
                animationController?.SetWindingUp(false);
                animationController?.SetRecovering(false);
                break;

            case SpinnerState.Windup:
                stateTimer = windupDuration;

                animationController?.SetSpinning(false);
                animationController?.SetWindingUp(true);
                animationController?.SetRecovering(false);

                telegraph?.Begin(windupDuration);
                break;

            case SpinnerState.Spinning:
                stateTimer = spinDuration;
                nextSpinAllowedTime = Time.time + spinCooldown;
                nextSpinHitTime = Time.time + spinHitStartDelay;

                animationController?.SetWindingUp(false);
                animationController?.SetSpinning(true);
                animationController?.SetRecovering(false);

                telegraph?.End();
                break;

            case SpinnerState.Recovering:
                stateTimer = recoverDuration;

                animationController?.SetSpinning(false);
                animationController?.SetWindingUp(false);
                animationController?.SetRecovering(true);

                telegraph?.End();
                break;
        }
    }
}
