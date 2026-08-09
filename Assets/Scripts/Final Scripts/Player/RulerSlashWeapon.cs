using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulerSlashWeapon :PlayerWeaponBase
{
    [Header("Melee Slash")]
    [SerializeField] private Transform slashPoint;
    [SerializeField] private float slashRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Ranged Slash Wave")]
    [SerializeField] private GameObject slashWavePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float slashWaveSpeed = 11f;
    [SerializeField] private float slashWaveLifetime = 3f;
    [SerializeField] private float homingTurnSpeed = 360f;

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
        if (direction == Vector2.zero)
            direction = transform.right;

        direction.Normalize();

        DoMeleeSlash();
        FireWavePattern(direction, AttackTarget);
    }

    private void DoMeleeSlash()
    {
        Vector3 center = slashPoint != null
            ? slashPoint.position
            : transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            center,
            slashRadius,
            enemyLayer
        );

        HashSet<IDamageable> damagedEnemies =
            new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null ||
                damagedEnemies.Contains(damageable))
            {
                continue;
            }

            damagedEnemies.Add(damageable);

            damageable.TakeDamage(
                new DamageInfo(
                    GetFinalDamage(),
                    gameObject,
                    hit.transform.position
                )
            );
        }
    }

    private void FireWavePattern(
        Vector2 direction,
        Transform target)
    {
        if (slashWavePrefab == null)
            return;

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

        for (int repeat = 0; repeat < repeatShots; repeat++)
        {
            float delay = repeat * backToBackDelay;

            if (delay <= 0f)
            {
                FireWaveVolley(direction, target, sideShots);
            }
            else
            {
                StartCoroutine(
                    FireWaveVolleyDelayed(
                        direction,
                        target,
                        sideShots,
                        delay
                    )
                );
            }
        }
    }

    private IEnumerator FireWaveVolleyDelayed(
        Vector2 direction,
        Transform target,
        int sideShots,
        float delay)
    {
        yield return new WaitForSeconds(delay);

        FireWaveVolley(direction, target, sideShots);
    }

    private void FireWaveVolley(
        Vector2 direction,
        Transform target,
        int sideShots)
    {
        if (sideShots <= 1)
        {
            SpawnSlashWave(
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

            SpawnSlashWave(direction, offset, target);
        }
    }

    private void SpawnSlashWave(
        Vector2 direction,
        Vector2 offset,
        Transform target)
    {
        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position;

        spawnPosition += (Vector3)offset;

        GameObject waveObject =
            ProjectilePoolProvider.Instance != null
                ? ProjectilePoolProvider.Instance.Spawn(
                    slashWavePrefab,
                    spawnPosition,
                    Quaternion.identity
                )
                : Instantiate(
                    slashWavePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

        RulerSlashWave wave = waveObject != null
            ? waveObject.GetComponent<RulerSlashWave>()
            : null;

        if (wave == null)
            return;

        int pierce =
            basePierceCount +
            (stats != null ? stats.PierceCount : 0);

        int enemyBounce =
            baseEnemyBounceCount +
            (stats != null ? stats.EnemyBounceCount : 0);

        int wallBounce =
            baseWallBounceCount +
            (stats != null ? stats.WallBounceCount : 0);

        wave.Launch(
            direction,
            GetFinalDamage(),
            slashWaveSpeed,
            slashWaveLifetime,
            pierce,
            enemyBounce,
            wallBounce,
            gameObject
        );

        wave.SetHomingTarget(
            target,
            homingTurnSpeed
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = slashPoint != null
            ? slashPoint.position
            : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, slashRadius);
    }
}
