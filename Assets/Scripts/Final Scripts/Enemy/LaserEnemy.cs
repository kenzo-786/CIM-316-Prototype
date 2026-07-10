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

    [Header("Laser")]
    [SerializeField] private float laserLength = 14f;
    [SerializeField] private float laserWidth = 0.35f;
    [SerializeField] private float aimDuration = 0.5f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float fireDuration = 0.18f;
    [SerializeField] private float cooldownDuration = 1.1f;
    [SerializeField] private LayerMask playerLayer;

    private LineRenderer line;
    private LaserState state;
    private float stateTimer;
    private Vector2 fireDirection = Vector2.right;
    private bool damageApplied;

    protected override void Awake()
    {
        base.Awake();
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnterState(LaserState.Aiming);
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        stateTimer -= Time.fixedDeltaTime;

        if (state == LaserState.Aiming)
        {
            MoveToward(target.position);
            UpdateDirectionToPlayer();

            if (stateTimer <= 0f)
                EnterState(LaserState.Charging);
        }
        else if (state == LaserState.Charging)
        {
            StopMoving();
            UpdateDirectionToPlayer();
            DrawLaserPreview(Mathf.InverseLerp(chargeDuration, 0f, stateTimer));

            if (stateTimer <= 0f)
                EnterState(LaserState.Firing);
        }
        else if (state == LaserState.Firing)
        {
            StopMoving();
            DrawLaserFire();

            if (!damageApplied)
                ApplyLaserDamage();

            if (stateTimer <= 0f)
                EnterState(LaserState.Cooldown);
        }
        else
        {
            StopMoving();

            if (stateTimer <= 0f)
                EnterState(LaserState.Aiming);
        }
    }

    private void UpdateDirectionToPlayer()
    {
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;

        if (direction != Vector2.zero)
            fireDirection = direction;
    }

    private void DrawLaserPreview(float chargePercent)
    {
        line.enabled = true;
        line.startWidth = laserWidth * 0.45f;
        line.endWidth = laserWidth * 0.45f;

        Color color = new Color(1f, 0f, 0f, Mathf.Lerp(0.18f, 0.75f, chargePercent));
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
        Vector3 end = start + (Vector3)(fireDirection * laserLength);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void ApplyLaserDamage()
    {
        damageApplied = true;

        Vector2 center = rb.position + fireDirection * laserLength * 0.5f;
        Vector2 size = new Vector2(laserLength, laserWidth);
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle, playerLayer);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = GetDamageable(hit);
            if (damageable != null)
                damageable.TakeDamage(new DamageInfo(ContactDamage, gameObject, hit.transform.position));
        }
    }

    private void EnterState(LaserState nextState)
    {
        state = nextState;
        damageApplied = false;

        if (state == LaserState.Aiming)
        {
            line.enabled = false;
            stateTimer = aimDuration;
        }
        else if (state == LaserState.Charging)
        {
            stateTimer = chargeDuration;
        }
        else if (state == LaserState.Firing)
        {
            stateTimer = fireDuration;
        }
        else
        {
            line.enabled = false;
            stateTimer = cooldownDuration;
        }
    }
}
