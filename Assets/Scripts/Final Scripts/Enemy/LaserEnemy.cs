using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEnemy : EnemyBase
{
    [Header("Laser")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float aimDuration = 1.4f;
    [SerializeField] private float fireDuration = 0.25f;
    [SerializeField] private float cooldown = 1.5f;
    [SerializeField] private float laserLength = 18f;
    [SerializeField] private float laserWidth = 0.35f;
    [SerializeField] private float laserDamage = 20f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement")]
    [SerializeField] private float preferredRange = 7f;

    private LineRenderer line;
    private Vector2 lockedDirection;
    private bool attacking;

    protected override void Awake()
    {
        base.Awake();

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;
        line.useWorldSpace = true;
    }

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);
        StartCoroutine(AttackRoutine());
    }

    private void FixedUpdate()
    {
        if (target == null || enemyData == null || IsDead || attacking) return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > preferredRange)
        {
            Vector2 direction = target.position - transform.position;
            rb.linearVelocity = direction.normalized * enemyData.moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (!IsDead)
        {
            attacking = true;
            rb.linearVelocity = Vector2.zero;

            line.enabled = true;

            float timer = 0f;

            while (timer < aimDuration)
            {
                timer += Time.deltaTime;

                if (target != null)
                    lockedDirection = ((Vector2)target.position - GetFirePosition()).normalized;

                float alpha = Mathf.Lerp(0.2f, 1f, timer / aimDuration);
                SetLineColor(new Color(1f, 0f, 0f, alpha));
                DrawLaserLine(lockedDirection);

                yield return null;
            }

            SetLineColor(Color.red);

            float fireTimer = 0f;

            while (fireTimer < fireDuration)
            {
                fireTimer += Time.deltaTime;
                DrawLaserLine(lockedDirection);
                DamageAlongLaser();
                yield return null;
            }

            line.enabled = false;
            attacking = false;

            yield return new WaitForSeconds(cooldown);
        }
    }

    private Vector2 GetFirePosition()
    {
        return firePoint != null ? firePoint.position : transform.position;
    }

    private void DrawLaserLine(Vector2 direction)
    {
        Vector2 start = GetFirePosition();
        Vector2 end = start + direction.normalized * laserLength;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void DamageAlongLaser()
    {
        Vector2 start = GetFirePosition();
        Vector2 center = start + lockedDirection.normalized * laserLength * 0.5f;
        float angle = Mathf.Atan2(lockedDirection.y, lockedDirection.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            new Vector2(laserLength, laserWidth),
            angle,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(new DamageInfo(
                    laserDamage,
                    gameObject,
                    hit.transform.position
                ));
            }
        }
    }

    private void SetLineColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 start = firePoint != null ? firePoint.position : transform.position;
        Gizmos.DrawLine(start, start + Vector2.right * laserLength);
    }
}
