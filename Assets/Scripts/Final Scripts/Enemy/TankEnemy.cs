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

    private TankState state;
    private float stateTimer;
    private float nextJumpTime;
    private Vector2 jumpStart;
    private Vector2 jumpEnd;
    private bool landed;

    protected override void OnEnable()
    {
        base.OnEnable();
        state = TankState.Moving;
        nextJumpTime = Time.time + jumpCooldown;
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (state == TankState.Moving)
        {
            MoveToward(target.position);

            if (Time.time >= nextJumpTime)
                StartWindup();
        }
        else if (state == TankState.Windup)
        {
            StopMoving();
            stateTimer -= Time.fixedDeltaTime;

            if (stateTimer <= 0f)
                StartJump();
        }
        else if (state == TankState.Jumping)
        {
            stateTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(stateTimer / jumpDuration);
            rb.MovePosition(Vector2.Lerp(jumpStart, jumpEnd, t));

            if (t >= 1f && !landed)
                Land();
        }
        else
        {
            StopMoving();
            stateTimer -= Time.fixedDeltaTime;

            if (stateTimer <= 0f)
            {
                state = TankState.Moving;
                nextJumpTime = Time.time + jumpCooldown;
            }
        }
    }

    private void StartWindup()
    {
        state = TankState.Windup;
        stateTimer = jumpWindup;
    }

    private void StartJump()
    {
        state = TankState.Jumping;
        stateTimer = 0f;
        landed = false;
        jumpStart = rb.position;
        jumpEnd = target.position;
    }

    private void Land()
    {
        landed = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, landingRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = GetDamageable(hit);
            if (damageable != null)
                damageable.TakeDamage(new DamageInfo(ContactDamage * landingDamageMultiplier, gameObject, hit.transform.position));
        }

        state = TankState.Recovering;
        stateTimer = recoverDuration;
    }
}
