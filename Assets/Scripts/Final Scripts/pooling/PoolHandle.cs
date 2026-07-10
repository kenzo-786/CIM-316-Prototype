using UnityEngine;
using UnityEngine.Pool;

public class PoolHandle : MonoBehaviour
{
    private ObjectPool owner;

    public bool HasOwner => owner != null;

    public void SetOwner(ObjectPool pool)
    {
        owner = pool;
    }

    public void ReturnToPool()
    {
        if (owner != null)
            owner.Release(gameObject);
        else
            Destroy(gameObject);
    }
}
