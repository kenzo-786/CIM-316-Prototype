using System.Collections.Generic;
using UnityEngine;

public class EraserThrowWeapon : PlayerWeaponBase
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 4f;

    [Header("Upgrade Driven")]
    [SerializeField] private int pierceCount;
    [SerializeField] private int enemyBounceCount;
    [SerializeField] private int wallBounceCount;
    [SerializeField] private int backToBackShots;
    [SerializeField] private int sideBySideShots;
    [SerializeField] private float backToBackDelay = 0.08f;
    [SerializeField] private float sideOffset = 0.35f;

    protected override void Attack(Vector2 direction)
    {
        if (projectilePrefab == null)
            return;

        if (direction == Vector2.zero)
            direction = transform.right;

        direction.Normalize();

        int sideShots = Mathf.Max(1, sideBySideShots);
        int repeatShots = Mathf.Max(1, backToBackShots);

        for (int repeat = 0; repeat < repeatShots; repeat++)
        {
            float delay = repeat * backToBackDelay;
            InvokeProjectileVolley(direction, sideShots, delay);
        }
    }

    private void InvokeProjectileVolley(Vector2 direction, int sideShots, float delay)
    {
        if (delay <= 0f)
        {
            FireProjectileVolley(direction, sideShots);
            return;
        }

        StartCoroutine(FireProjectileVolleyDelayed(direction, sideShots, delay));
    }

    private System.Collections.IEnumerator FireProjectileVolleyDelayed(Vector2 direction, int sideShots, float delay)
    {
        yield return new WaitForSeconds(delay);
        FireProjectileVolley(direction, sideShots);
    }

    private void FireProjectileVolley(Vector2 direction, int sideShots)
    {
        if (sideShots <= 1)
        {
            SpawnProjectile(direction, Vector2.zero);
            return;
        }

        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float startOffset = -(sideShots - 1) * sideOffset * 0.5f;

        for (int i = 0; i < sideShots; i++)
        {
            Vector2 offset = perpendicular * (startOffset + i * sideOffset);
            SpawnProjectile(direction, offset);
        }
    }

    private void SpawnProjectile(Vector2 direction, Vector2 offset)
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        spawnPosition += (Vector3)offset;

        GameObject projectileObject = ProjectilePoolProvider.Instance != null
            ? ProjectilePoolProvider.Instance.Spawn(projectilePrefab, spawnPosition, Quaternion.identity)
            : Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        EraserProjectile projectile = projectileObject != null
            ? projectileObject.GetComponent<EraserProjectile>()
            : null;

        if (projectile == null)
            return;

        projectile.Launch(
            direction,
            GetFinalDamage(),
            projectileSpeed,
            projectileLifetime,
            pierceCount,
            enemyBounceCount,
            wallBounceCount,
            gameObject
        );
    }

    public void SetProjectileModifiers(
        int newPierceCount,
        int newEnemyBounceCount,
        int newWallBounceCount,
        int newBackToBackShots,
        int newSideBySideShots)
    {
        pierceCount = Mathf.Max(0, newPierceCount);
        enemyBounceCount = Mathf.Max(0, newEnemyBounceCount);
        wallBounceCount = Mathf.Max(0, newWallBounceCount);
        backToBackShots = Mathf.Max(1, newBackToBackShots);
        sideBySideShots = Mathf.Max(1, newSideBySideShots);
    }
}
