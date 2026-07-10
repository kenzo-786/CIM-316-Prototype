using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private bool allowGrowth = true;
    [SerializeField] private int maxSize = 200;
    [SerializeField] private Transform inactiveRoot;

    private readonly Queue<GameObject> inactiveObjects = new Queue<GameObject>();
    private readonly HashSet<GameObject> allObjects = new HashSet<GameObject>();
    private bool warmed;

    public GameObject Prefab => prefab;
    public int CountInactive => inactiveObjects.Count;
    public int CountTotal => allObjects.Count;

    private void Awake()
    {
        Warm();
    }

    public void Initialize(GameObject poolPrefab, int prewarmCount, bool canGrow, int maximumSize)
    {
        prefab = poolPrefab;
        initialSize = Mathf.Max(0, prewarmCount);
        allowGrowth = canGrow;
        maxSize = Mathf.Max(1, maximumSize);
        Warm();
    }

    public void Warm()
    {
        if (warmed || prefab == null)
            return;

        warmed = true;

        if (inactiveRoot == null)
            inactiveRoot = transform;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject instance = CreateObject();
            inactiveObjects.Enqueue(instance);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        Warm();

        GameObject instance = inactiveObjects.Count > 0 ? inactiveObjects.Dequeue() : TryCreateObject();

        if (instance == null)
            return null;

        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(parent, false);
        instanceTransform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        foreach (IPoolable poolable in poolables)
            poolable.OnSpawnedFromPool();

        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null || !allObjects.Contains(instance))
            return;

        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        foreach (IPoolable poolable in poolables)
            poolable.OnReturnedToPool();

        instance.SetActive(false);
        instance.transform.SetParent(inactiveRoot != null ? inactiveRoot : transform, false);
        inactiveObjects.Enqueue(instance);
    }

    private GameObject TryCreateObject()
    {
        if (!allowGrowth && allObjects.Count >= maxSize)
            return null;

        if (allObjects.Count >= maxSize)
            return null;

        return CreateObject();
    }

    private GameObject CreateObject()
    {
        GameObject instance = Instantiate(prefab, inactiveRoot != null ? inactiveRoot : transform);
        instance.SetActive(false);

        PoolHandle handle = instance.GetComponent<PoolHandle>();
        if (handle == null)
            handle = instance.AddComponent<PoolHandle>();

        handle.SetOwner(this);
        allObjects.Add(instance);
        return instance;
    }
}
