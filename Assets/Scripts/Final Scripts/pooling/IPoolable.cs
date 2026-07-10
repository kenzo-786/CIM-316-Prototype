using UnityEngine;

public interface IPoolable
{
    void OnSpawnedFromPool();
    void OnReturnedToPool();
}
