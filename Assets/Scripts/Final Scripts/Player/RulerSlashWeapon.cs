using UnityEngine;

public class RulerSlashWeapon :PlayerWeaponBase
{
    [Header("Melee Slash")]
    [SerializeField] private Transform slashOrigin;
    [SerializeField] private float range = 1.8f;
    [SerializeField] private float arcAngle = 100f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Slash Wave")]
    [SerializeField] private RulerSlashWave slashWavePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float waveSpeed = 10f;
    [SerializeField] private float waveLifetime = 1.5f;
    [SerializeField] private LayerMask wallBounceLayer;
    [SerializeField] private LayerMask destroyLayer;

    [Header("Multi Fire")]
    [SerializeField] private float sideAngle = 10f;

    protected override void Attack(Vector2 aimDirection)
    {
        DoMeleeSlash(aimDirection);
        FireSlashWave(aimDirection);

        if (stats == null) return;

        for (int i = 0; i < stats.SideProjectiles; i++)
        {
            float angle = sideAngle * (i + 1);
            FireSlashWave(Rotate(aimDirection, angle));
            FireSlashWave(Rotate(aimDirection, -angle));
        }

        for (int i = 0; i < stats.BackProjectiles; i++)
        {
            FireSlashWave(-aimDirection);
        }
    }

    private void DoMeleeSlash(Vector2 aimDirection)
    {
        Vector2 origin = slashOrigin != null ? slashOrigin.position : transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Vector2 directionToTarget = (Vector2)hit.transform.position - origin;
            float angle = Vector2.Angle(aimDirection, directionToTarget);

            if (angle > arcAngle * 0.5f)
                continue;

            if (hit.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(
                    new DamageInfo(GetFinalDamage(), gameObject, hit.transform.position)
                );
            }
        }
    }

    private void FireSlashWave(Vector2 direction)
    {
        if (slashWavePrefab == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        RulerSlashWave wave = Instantiate(
            slashWavePrefab,
            spawnPosition,
            Quaternion.identity
        );

        wave.Initialize(
            GetFinalDamage(),
            waveSpeed,
            waveLifetime,
            direction,
            gameObject,
            enemyLayer,
            wallBounceLayer,
            destroyLayer,
            stats != null ? stats.PierceCount : 0,
            stats != null ? stats.EnemyBounceCount : 0,
            stats != null ? stats.WallBounceCount : 0
        );
    }

    private Vector2 Rotate(Vector2 direction, float angle)
    {
        return Quaternion.Euler(0f, 0f, angle) * direction;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = slashOrigin != null ? slashOrigin.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, range);
    }
}
