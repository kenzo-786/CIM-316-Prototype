using UnityEngine;

public class EraserThrowWeapon : PlayerWeaponBase
{
    [SerializeField] private EraserProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 9f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallBounceLayer;
    [SerializeField] private LayerMask destroyLayer;

    [Header("Multi Fire")]
    [SerializeField] private float sideAngle = 12f;

    protected override void Attack(Vector2 aimDirection)
    {
        FireProjectile(aimDirection);

        if (stats == null) return;

        for (int i = 0; i < stats.SideProjectiles; i++)
        {
            float angle = sideAngle * (i + 1);
            FireProjectile(Rotate(aimDirection, angle));
            FireProjectile(Rotate(aimDirection, -angle));
        }

        for (int i = 0; i < stats.BackProjectiles; i++)
        {
            FireProjectile(-aimDirection);
        }
    }

    private void FireProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        EraserProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Initialize(
            GetFinalDamage(),
            projectileSpeed,
            projectileLifetime,
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
}
