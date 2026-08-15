using UnityEngine;

public class TankEnemy : EnemyBase
{
    private enum TankState
    {
        Moving,
        Windup,
        Jumping,
        Recovering
    }

    [Header("Tank")]
    [SerializeField] private float jumpCooldown = 2.5f;
    [SerializeField] private float jumpWindup = 0.65f;
    [SerializeField] private float attackAnimationDuration = 0.38f;
    [SerializeField] private float recoverDuration = 0.5f;
    [SerializeField] private float landingRadius = 1.5f;
    [SerializeField] private float landingDamageMultiplier = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private TankState state;
    private float stateTimer;
    private float nextJumpTime;
    private Vector2 jumpStart;
    private Vector2 jumpEnd;
    private Vector2 attackDirection = Vector2.down;
    private bool landed;

    private float JumpDuration => Mathf.Max(0.05f, attackAnimationDuration);

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

        state = TankState.Moving;
        nextJumpTime = Time.time + jumpCooldown;
        animationController?.SetStationary();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        switch (state)
        {
            case TankState.Moving:
                Vector2 toPlayer = (Vector2)target.position - rb.position;

                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    MoveInDirection(toPlayer);
                    animationController?.SetMovementDirection(toPlayer);
                }
                else
                {
                    StopMoving();
                    animationController?.SetStationary();
                }

                if (Time.time >= nextJumpTime)
                    StartWindup(toPlayer);

                break;

            case TankState.Windup:
                StopMoving();
                animationController?.SetStationary();
                animationController?.SetFacingDirection(attackDirection);

                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0f)
                    StartJump();

                break;

            case TankState.Jumping:
                StopMoving();
                animationController?.SetStationary();
                animationController?.SetFacingDirection(attackDirection);

                stateTimer += Time.fixedDeltaTime;

                float jumpProgress = Mathf.Clamp01(stateTimer / JumpDuration);

                rb.MovePosition(
                    Vector2.Lerp(
                        jumpStart,
                        jumpEnd,
                        jumpProgress
                    )
                );

                if (jumpProgress >= 1f && !landed)
                    Land();

                break;

            case TankState.Recovering:
                StopMoving();
                animationController?.SetStationary();
                animationController?.SetFacingDirection(attackDirection);

                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0f)
                {
                    state = TankState.Moving;
                    nextJumpTime = Time.time + jumpCooldown;
                }

                break;
        }
    }

    private void StartWindup(Vector2 toPlayer)
    {
        if (toPlayer.sqrMagnitude > 0.0001f)
            attackDirection = toPlayer.normalized;

        state = TankState.Windup;
        stateTimer = jumpWindup;
        jumpEnd = target.position;

        StopMoving();
        animationController?.SetStationary();
        animationController?.SetFacingDirection(attackDirection);
        if (telegraph != null)
        {
            telegraph.BeginAtPosition(
                jumpWindup + JumpDuration,
                jumpEnd,
                landingRadius * 2f
            );
        }
    }

    private void StartJump()
    {
        state = TankState.Jumping;
        stateTimer = 0f;
        landed = false;
        jumpStart = rb.position;
        animationController?.PlayAttack();
    }

    private void Land()
    {
        landed = true;

        if (telegraph != null)
            telegraph.End();

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            landingRadius,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = GetDamageable(hit);

            if (damageable != null)
            {
                damageable.TakeDamage(
                    new DamageInfo(
                        ContactDamage * landingDamageMultiplier,
                        gameObject,
                        hit.transform.position
                    )
                );
            }
        }

        state = TankState.Recovering;
        stateTimer = recoverDuration;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, landingRadius);
    }
}
