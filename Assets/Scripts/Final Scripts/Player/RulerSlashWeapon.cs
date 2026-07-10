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
        if (direction == Vector2.zero)
            direction = transform.right;

        direction.Normalize();

        DoMeleeSlash();
        FireWavePattern(direction);
    }

    private void DoMeleeSlash()
    {
        Vector3 center = slashPoint != null ? slashPoint.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, slashRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
                damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(new DamageInfo(GetFinalDamage(), gameObject, hit.transform.position));
        }
    }

    private void FireWavePattern(Vector2 direction)
    {
        if (slashWavePrefab == null)
            return;

        int sideShots = Mathf.Max(1, sideBySideShots);
        int repeatShots = Mathf.Max(1, backToBackShots);

        for (int repeat = 0; repeat < repeatShots; repeat++)
        {
            float delay = repeat * backToBackDelay;
            InvokeWaveVolley(direction, sideShots, delay);
        }
    }

    private void InvokeWaveVolley(Vector2 direction, int sideShots, float delay)
    {
        if (delay <= 0f)
        {
            FireWaveVolley(direction, sideShots);
            return;
        }

        StartCoroutine(FireWaveVolleyDelayed(direction, sideShots, delay));
    }

    private System.Collections.IEnumerator FireWaveVolleyDelayed(Vector2 direction, int sideShots, float delay)
    {
        yield return new WaitForSeconds(delay);
        FireWaveVolley(direction, sideShots);
    }

    private void FireWaveVolley(Vector2 direction, int sideShots)
    {
        if (sideShots <= 1)
        {
            SpawnSlashWave(direction, Vector2.zero);
            return;
        }

        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float startOffset = -(sideShots - 1) * sideOffset * 0.5f;

        for (int i = 0; i < sideShots; i++)
        {
            Vector2 offset = perpendicular * (startOffset + i * sideOffset);
            SpawnSlashWave(direction, offset);
        }
    }

    private void SpawnSlashWave(Vector2 direction, Vector2 offset)
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        spawnPosition += (Vector3)offset;

        GameObject waveObject = ProjectilePoolProvider.Instance != null
            ? ProjectilePoolProvider.Instance.Spawn(slashWavePrefab, spawnPosition, Quaternion.identity)
            : Instantiate(slashWavePrefab, spawnPosition, Quaternion.identity);

        RulerSlashWave wave = waveObject != null
            ? waveObject.GetComponent<RulerSlashWave>()
            : null;

        if (wave == null)
            return;

        wave.Launch(
            direction,
            GetFinalDamage(),
            slashWaveSpeed,
            slashWaveLifetime,
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

    private void OnDrawGizmosSelected()
    {
        Vector3 center = slashPoint != null ? slashPoint.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, slashRadius);
    }
}
