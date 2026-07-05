using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int preloadCount = 20;

    private readonly Queue<GameObject> available = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < preloadCount; i++)
            available.Enqueue(CreateObject());
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = available.Count > 0 ? available.Dequeue() : CreateObject();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnTakenFromPool();

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnReturnedToPool();

        obj.SetActive(false);
        available.Enqueue(obj);
    }

    private GameObject CreateObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);

        PoolHandle handle = obj.GetComponent<PoolHandle>();
        if (handle == null) handle = obj.AddComponent<PoolHandle>();
        handle.SetPool(this);

        return obj;
    }
}
