using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.7f, 0f);
    [SerializeField] private ProjectilePoolProvider poolProvider;

    private void Awake()
    {
        if (poolProvider == null)
            poolProvider = ProjectilePoolProvider.Instance;
    }

    private void OnEnable()
    {
        FeedbackEventBus.OnDamageNumberRequested += Spawn;
    }

    private void OnDisable()
    {
        FeedbackEventBus.OnDamageNumberRequested -= Spawn;
    }

    private void Spawn(Vector3 position, float amount, bool critical)
    {
        if (damageNumberPrefab == null)
            return;

        ProjectilePoolProvider provider = poolProvider != null ? poolProvider : ProjectilePoolProvider.Instance;

        GameObject numberObject = provider != null
            ? provider.Spawn(damageNumberPrefab, position + spawnOffset, Quaternion.identity)
            : Instantiate(damageNumberPrefab, position + spawnOffset, Quaternion.identity);

        DamageNumber damageNumber = numberObject != null
            ? numberObject.GetComponent<DamageNumber>()
            : null;

        if (damageNumber != null)
            damageNumber.Show(amount, critical);
    }
}
