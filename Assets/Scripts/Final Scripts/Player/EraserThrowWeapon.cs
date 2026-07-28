using System.Collections.Generic;
using UnityEngine;

public class EraserThrowWeapon : PlayerWeaponBase
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 4f;
    [SerializeField] private float homingTurnSpeed = 900f;

    [Header("Base Projectile Modifiers")]
    [SerializeField] private int basePierceCount;
    [SerializeField] private int baseEnemyBounceCount;
    [SerializeField] private int baseWallBounceCount;
    [SerializeField] private int baseBackToBackShots = 1;
    [SerializeField] private int baseSideBySideShots = 1;
    [SerializeField] private float backToBackDelay = 0.08f;
    [SerializeField] private float sideOffset = 0.35f;

    protected override void Attack(Vector2 direction)
    {
        if (projectilePrefab == null)
            return;

        if (direction == Vector2.zero)
            direction = transform.right;

        direction.Normalize();

        Transform target = AttackTarget;

        int sideShots = Mathf.Max(
            1,
            baseSideBySideShots +
            (stats != null ? stats.SideProjectiles : 0)
        );

        int repeatShots = Mathf.Max(
            1,
            baseBackToBackShots +
            (stats != null ? stats.BackProjectiles : 0)
        );

        for (int repeat = 0;
             repeat < repeatShots;
             repeat++)
        {
            float delay = repeat * backToBackDelay;

            InvokeProjectileVolley(
                direction,
                target,
                sideShots,
                delay
            );
        }
    }

    private void InvokeProjectileVolley(
        Vector2 direction,
        Transform target,
        int sideShots,
        float delay)
    {
        if (delay <= 0f)
        {
            FireProjectileVolley(
                direction,
                target,
                sideShots
            );

            return;
        }

        StartCoroutine(
            FireProjectileVolleyDelayed(
                direction,
                target,
                sideShots,
                delay
            )
        );
    }

    private System.Collections.IEnumerator
        FireProjectileVolleyDelayed(
            Vector2 direction,
            Transform target,
            int sideShots,
            float delay)
    {
        yield return new WaitForSeconds(delay);

        FireProjectileVolley(
            direction,
            target,
            sideShots
        );
    }

    private void FireProjectileVolley(
        Vector2 direction,
        Transform target,
        int sideShots)
    {
        if (sideShots <= 1)
        {
            SpawnProjectile(
                direction,
                Vector2.zero,
                target
            );

            return;
        }

        Vector2 perpendicular =
            new Vector2(-direction.y, direction.x);

        float startOffset =
            -(sideShots - 1) * sideOffset * 0.5f;

        for (int i = 0; i < sideShots; i++)
        {
            Vector2 offset =
                perpendicular *
                (startOffset + i * sideOffset);

            SpawnProjectile(
                direction,
                offset,
                target
            );
        }
    }

    private void SpawnProjectile(
        Vector2 direction,
        Vector2 offset,
        Transform target)
    {
        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position;

        spawnPosition += (Vector3)offset;

        GameObject projectileObject =
            ProjectilePoolProvider.Instance != null
                ? ProjectilePoolProvider.Instance.Spawn(
                    projectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                )
                : Instantiate(
                    projectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

        EraserProjectile projectile =
            projectileObject != null
                ? projectileObject
                    .GetComponent<EraserProjectile>()
                : null;

        if (projectile == null)
            return;

        int pierce =
            basePierceCount +
            (stats != null ? stats.PierceCount : 0);

        int enemyBounce =
            baseEnemyBounceCount +
            (stats != null
                ? stats.EnemyBounceCount
                : 0);

        int wallBounce =
            baseWallBounceCount +
            (stats != null
                ? stats.WallBounceCount
                : 0);

        projectile.Launch(
            direction,
            GetFinalDamage(),
            projectileSpeed,
            projectileLifetime,
            pierce,
            enemyBounce,
            wallBounce,
            gameObject
        );

        projectile.SetHomingTarget(
            target,
            homingTurnSpeed
        );
    }
}
