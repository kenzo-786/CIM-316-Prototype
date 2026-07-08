using UnityEngine;

public class EnemyProjectileShooter : MonoBehaviour
{
    [SerializeField] private EnemyProjectileData projectileData;
    [SerializeField] private Transform firePoint;

    public void ShootAt(Vector3 targetPosition)
    {
        if (projectileData == null || projectileData.prefab == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = targetPosition - spawnPosition;

        GameObject projectileObject = Instantiate(
            projectileData.prefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
        projectile.Initialize(projectileData, direction);
    }
}
