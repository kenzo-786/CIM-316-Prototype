using UnityEngine;

public class PooledTimedEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 1f;

    private float timer;

    public void OnSpawnedFromPool()
    {
        timer = lifetime;
    }

    public void OnReturnedToPool()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
            PooledProjectileUtility.Despawn(gameObject);
    }
}
