using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolProvider : MonoBehaviour
{
    [System.Serializable]
    private class ProjectilePoolEntry
    {
        public GameObject prefab;
        public int prewarmCount = 30;
        public int maxSize = 250;
    }

    [SerializeField] private List<ProjectilePoolEntry> prewarmPools = new List<ProjectilePoolEntry>();
    [SerializeField] private int defaultPrewarmCount = 20;
    [SerializeField] private int defaultMaxSize = 250;
    [SerializeField] private bool createPoolsOnDemand = true;

    private readonly Dictionary<GameObject, ObjectPool> poolsByPrefab = new Dictionary<GameObject, ObjectPool>();

    public static ProjectilePoolProvider Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (ProjectilePoolEntry entry in prewarmPools)
        {
            if (entry != null && entry.prefab != null)
                GetOrCreatePool(entry.prefab, entry.prewarmCount, entry.maxSize);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        ObjectPool pool = GetOrCreatePool(prefab, defaultPrewarmCount, defaultMaxSize);

        if (pool == null)
            return Instantiate(prefab, position, rotation);

        return pool.Get(position, rotation);
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null)
            return;

        PoolHandle handle = instance.GetComponent<PoolHandle>();

        if (handle != null && handle.HasOwner)
            handle.ReturnToPool();
        else
            Destroy(instance);
    }

    private ObjectPool GetOrCreatePool(GameObject prefab, int prewarmCount, int maxSize)
    {
        if (poolsByPrefab.TryGetValue(prefab, out ObjectPool existingPool))
            return existingPool;

        if (!createPoolsOnDemand)
            return null;

        GameObject poolObject = new GameObject(prefab.name + " Pool");
        poolObject.transform.SetParent(transform, false);

        ObjectPool pool = poolObject.AddComponent<ObjectPool>();
        pool.Initialize(prefab, prewarmCount, true, maxSize);
        poolsByPrefab.Add(prefab, pool);
        return pool;
    }
}
