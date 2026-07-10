using UnityEngine;

public static class PooledProjectileUtility 
{
    public static void Despawn(GameObject projectileObject)
    {
        if (projectileObject == null)
            return;

        PoolHandle handle = projectileObject.GetComponent<PoolHandle>();

        if (handle != null && handle.HasOwner)
            handle.ReturnToPool();
        else
            Object.Destroy(projectileObject);
    }
}
