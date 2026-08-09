using UnityEngine;
using System.Collections;

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
    [SerializeField] private float jumpWindup = 0.55f;
    [SerializeField] private float jumpDuration = 0.45f;
    [SerializeField] private float recoverDuration = 0.5f;
    [SerializeField] private float landingRadius = 1.5f;
    [SerializeField] private float landingDamageMultiplier = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private EnemyTelegraphFeedback telegraph;

    private TankState state;
    private float stateTimer;
    private float nextJumpTime;
    private Vector2 jumpStart;
    private Vector2 jumpEnd;
    private bool landed;

    protected override void Awake()
    {
        base.Awake();

        if (telegraph == null)
        {
            telegraph = GetComponent<EnemyTelegraphFeedback>();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        state = TankState.Moving;
        nextJumpTime = Time.time + jumpCooldown;
    }

    protected override void TickEnemy()
    {
        if (target == null)
        {
            return;
        }

        switch (state)
        {
            case TankState.Moving:
                MoveToward(target.position);

                if (Time.time >= nextJumpTime)
                {
                    StartWindup();
                }

                break;

            case TankState.Windup:
                StopMoving();

                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0f)
                {
                    StartJump();
                }

                break;

            case TankState.Jumping:
                stateTimer += Time.fixedDeltaTime;

                float t =
                    Mathf.Clamp01(
                        stateTimer /
                        jumpDuration
                    );

                rb.MovePosition(
                    Vector2.Lerp(
                        jumpStart,
                        jumpEnd,
                        t
                    )
                );

                if (t >= 1f && !landed)
                {
                    Land();
                }

                break;

            case TankState.Recovering:
                StopMoving();

                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0f)
                {
                    state = TankState.Moving;
                    nextJumpTime = Time.time + jumpCooldown;
                }

                break;
        }
    }

    private void StartWindup()
    {
        state = TankState.Windup;
        stateTimer = jumpWindup;
        jumpEnd = target.position;

        if (telegraph != null)
        {
            telegraph.BeginAtPosition(
                jumpWindup + jumpDuration,
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
    }

    private void Land()
    {
        landed = true;

        if (telegraph != null)
        {
            telegraph.End();
        }

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                landingRadius,
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
                        ContactDamage *
                        landingDamageMultiplier,
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

        Gizmos.DrawWireSphere(
            transform.position,
            landingRadius
        );
    }
}
