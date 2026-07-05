using UnityEngine;
using UnityEngine.Pool;

public class PoolHandle : MonoBehaviour
{
    private ObjectPool pool;

    public void SetPool(ObjectPool owner)
    {
        pool = owner;
    }

    public void ReturnToPool()
    {
        if (pool != null) pool.Return(gameObject);
        else Destroy(gameObject);
    }
}
