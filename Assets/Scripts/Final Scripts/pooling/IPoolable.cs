using UnityEngine;

public interface IPoolable
{
    void OnTakenFromPool();
    void OnReturnedToPool();
}
