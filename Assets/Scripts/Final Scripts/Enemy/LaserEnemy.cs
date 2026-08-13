using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEnemy : EnemyBase
{
    private enum LaserState
    {
        Aiming,
        Charging,
        Firing,
        Cooldown
    }

    [Header("Positioning")]
    [SerializeField] private float preferredRange = 9f;
    [SerializeField] private float rangeTolerance = 1.5f;
    [SerializeField] private float strafeSpeedMultiplier = 0.8f;
    [SerializeField] private float strafeChangeInterval = 1.5f;
    [SerializeField] private float distanceCorrectionStrength = 1.25f;

    [Header("Laser")]
    [SerializeField] private float laserLength = 50f;
    [SerializeField] private float laserWidth = 0.35f;
    [SerializeField] private float aimDuration = 0.65f;
    [SerializeField] private float chargeDuration = 1.3f;
    [SerializeField] private float aimLockDuration = 0.4f;
    [SerializeField] private float fireDuration = 0.18f;
    [SerializeField] private float cooldownDuration = 1.1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private LineRenderer line;
    private LaserState state;
    private Vector2 fireDirection = Vector2.right;
    private float stateTimer;
    private float nextStrafeChangeTime;
    private int strafeDirection = 1;
    private bool damageApplied;

    protected override void Awake()
    {
        base.Awake();

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.enabled = false;

        if (telegraph == null)
            telegraph = GetComponent<EnemyTelegraphFeedback>();

        if (animationController == null)
        {
            animationController =
                GetComponentInChildren<EnemyAnimationController>();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        strafeDirection =
            Random.value < 0.5f ? -1 : 1;

        nextStrafeChangeTime =
            Time.time + strafeChangeInterval;

        if (line != null)
            line.enabled = false;

        animationController?.SetStationary();

        EnterState(LaserState.Aiming);
    }

    protected override void OnDisable()
    {
        if (line != null)
            line.enabled = false;

        animationController?.SetStationary();

        base.OnDisable();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        stateTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case LaserState.Aiming:
                MaintainDistanceAndStrafe();
                UpdateDirectionToPlayer();

                if (stateTimer <= 0f)
                    EnterState(LaserState.Charging);

                break;

            case LaserState.Charging:
                StopMoving();
                animationController?.SetStationary();

                if (stateTimer > aimLockDuration)
                    UpdateDirectionToPlayer();

                animationController?.SetFacingDirection(
                    fireDirection
                );

                DrawLaserPreview();

                if (stateTimer <= 0f)
                    EnterState(LaserState.Firing);

                break;

            case LaserState.Firing:
                StopMoving();
                animationController?.SetStationary();

                animationController?.SetFacingDirection(
                    fireDirection
                );

                DrawLaserFire();

                if (!damageApplied)
                    ApplyLaserDamage();

                if (stateTimer <= 0f)
                    EnterState(LaserState.Cooldown);

                break;

            case LaserState.Cooldown:
                MaintainDistanceAndStrafe();

                if (stateTimer <= 0f)
                    EnterState(LaserState.Aiming);

                break;
        }
    }

    private void MaintainDistanceAndStrafe()
    {
        Vector2 toPlayer =
            (Vector2)target.position - rb.position;

        float distance = toPlayer.magnitude;

        if (distance <= 0.01f)
        {
            StopMoving();
            animationController?.SetStationary();
            return;
        }

        if (Time.time >= nextStrafeChangeTime)
        {
            strafeDirection *= -1;

            nextStrafeChangeTime =
                Time.time + strafeChangeInterval;
        }

        Vector2 radial = toPlayer.normalized;

        Vector2 sideways =
            new Vector2(-radial.y, radial.x) *
            strafeDirection;

        float correction = 0f;

        if (distance < preferredRange - rangeTolerance)
            correction = -1f;
        else if (distance > preferredRange + rangeTolerance)
            correction = 1f;

        Vector2 movement =
            (
                sideways +
                radial *
                correction *
                distanceCorrectionStrength
            ).normalized;

        MoveInDirection(
            movement,
            strafeSpeedMultiplier
        );

        animationController?.SetMovementDirection(
            movement
        );
    }

    private void UpdateDirectionToPlayer()
    {
        Vector2 direction =
            (
                (Vector2)target.position -
                rb.position
            ).normalized;

        if (direction != Vector2.zero)
            fireDirection = direction;
    }

    private void DrawLaserPreview()
    {
        line.enabled = true;
        line.startWidth = laserWidth * 0.45f;
        line.endWidth = laserWidth * 0.45f;

        bool aimLocked =
            stateTimer <= aimLockDuration;

        float chargeProgress =
            1f -
            Mathf.Clamp01(
                stateTimer / chargeDuration
            );

        float alpha = aimLocked
            ? 0.95f
            : Mathf.Lerp(
                0.2f,
                0.75f,
                chargeProgress
            );

        Color color =
            new Color(1f, 0f, 0f, alpha);

        line.startColor = color;
        line.endColor = color;

        SetLinePositions();
    }

    private void DrawLaserFire()
    {
        line.enabled = true;
        line.startWidth = laserWidth;
        line.endWidth = laserWidth;
        line.startColor = Color.red;
        line.endColor = Color.red;

        SetLinePositions();
    }

    private void SetLinePositions()
    {
        Vector3 start = transform.position;

        Vector3 end =
            start +
            (Vector3)(
                fireDirection *
                laserLength
            );

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void ApplyLaserDamage()
    {
        damageApplied = true;

        Vector2 center =
            rb.position +
            fireDirection *
            laserLength *
            0.5f;

        Vector2 size =
            new Vector2(
                laserLength,
                laserWidth
            );

        float angle =
            Mathf.Atan2(
                fireDirection.y,
                fireDirection.x
            ) *
            Mathf.Rad2Deg;

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                center,
                size,
                angle,
                playerLayer
            );

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable =
                GetDamageable(hit);

            if (damageable != null)
            {
                damageable.TakeDamage(
                    new DamageInfo(
                        ContactDamage,
                        gameObject,
                        hit.transform.position
                    )
                );
            }
        }
    }

    private void EnterState(LaserState nextState)
    {
        state = nextState;
        damageApplied = false;

        switch (state)
        {
            case LaserState.Aiming:
                line.enabled = false;
                stateTimer = aimDuration;
                telegraph?.End();
                break;

            case LaserState.Charging:
                stateTimer = chargeDuration;
                telegraph?.Begin(chargeDuration);
                break;

            case LaserState.Firing:
                stateTimer = fireDuration;
                telegraph?.End();
                break;

            case LaserState.Cooldown:
                line.enabled = false;
                stateTimer = cooldownDuration;
                telegraph?.End();
                break;
        }
    }
}
