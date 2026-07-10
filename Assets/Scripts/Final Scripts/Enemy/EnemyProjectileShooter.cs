using UnityEngine;

public class EnemyProjectileShooter : MonoBehaviour
{
    [SerializeField] private EnemyProjectileData projectileData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ProjectilePoolProvider poolProvider;

    public EnemyProjectileData ProjectileData => projectileData;

    private void Awake()
    {
        if (poolProvider == null)
            poolProvider = ProjectilePoolProvider.Instance;
    }

    public void SetProjectileData(EnemyProjectileData data)
    {
        projectileData = data;
    }

    public void ShootAt(Vector2 targetPosition, float damageMultiplier, GameObject owner)
    {
        Vector2 startPosition = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = (targetPosition - startPosition).normalized;
        ShootDirection(direction, damageMultiplier, owner);
    }

    public void ShootDirection(Vector2 direction, float damageMultiplier, GameObject owner)
    {
        if (projectileData == null || projectileData.prefab == null)
            return;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        Vector2 startPosition = firePoint != null ? firePoint.position : transform.position;
        ProjectilePoolProvider provider = poolProvider != null ? poolProvider : ProjectilePoolProvider.Instance;
        GameObject projectileObject = provider != null
            ? provider.Spawn(projectileData.prefab, startPosition, Quaternion.identity)
            : Instantiate(projectileData.prefab, startPosition, Quaternion.identity);

        EnemyProjectile projectile = projectileObject != null ? projectileObject.GetComponent<EnemyProjectile>() : null;

        if (projectile != null)
            projectile.Launch(projectileData, direction, damageMultiplier, owner);
    }

    public void ShootSpread(Vector2 centerDirection, int projectileCount, float spreadAngle, float damageMultiplier, GameObject owner)
    {
        projectileCount = Mathf.Max(1, projectileCount);

        if (projectileCount == 1)
        {
            ShootDirection(centerDirection, damageMultiplier, owner);
            return;
        }

        float startAngle = -spreadAngle * 0.5f;
        float step = spreadAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + step * i;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * centerDirection;
            ShootDirection(direction, damageMultiplier, owner);
        }
    }
}
