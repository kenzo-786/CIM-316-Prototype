using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    [SerializeField] private ProjectilePoolProvider poolProvider;

    private void Awake()
    {
        if (poolProvider == null)
            poolProvider = ProjectilePoolProvider.Instance;
    }

    private void OnEnable()
    {
        FeedbackEventBus.OnEffectRequested += Spawn;
    }

    private void OnDisable()
    {
        FeedbackEventBus.OnEffectRequested -= Spawn;
    }

    private void Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
            return;

        ProjectilePoolProvider provider = poolProvider != null ? poolProvider : ProjectilePoolProvider.Instance;

        if (provider != null)
            provider.Spawn(prefab, position, Quaternion.identity);
        else
            Instantiate(prefab, position, Quaternion.identity);
    }
}
