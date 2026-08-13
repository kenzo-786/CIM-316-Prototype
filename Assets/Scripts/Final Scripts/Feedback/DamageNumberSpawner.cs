using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject damageNumberPrefab;

    [Header("Position")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.7f, 0f);

    [Header("Pooling")]
    [SerializeField] private ProjectilePoolProvider poolProvider;

    private void Awake()
    {
        if (poolProvider == null)
            poolProvider = ProjectilePoolProvider.Instance;
    }

    private void OnEnable()
    {
        FeedbackEventBus.OnDamageNumberRequested += SpawnDamage;
        FeedbackEventBus.OnHealingNumberRequested += SpawnHealing;
        FeedbackEventBus.OnWorldTextRequested += SpawnMessage;
    }

    private void OnDisable()
    {
        FeedbackEventBus.OnDamageNumberRequested -= SpawnDamage;
        FeedbackEventBus.OnHealingNumberRequested -= SpawnHealing;
        FeedbackEventBus.OnWorldTextRequested -= SpawnMessage;
    }

    private void SpawnDamage(
        Vector3 position,
        float amount,
        DamageType damageType)
    {
        DamageNumber number = Spawn(position);

        if (number != null)
            number.Show(amount, damageType);
    }

    private void SpawnHealing(Vector3 position, float amount)
    {
        DamageNumber number = Spawn(position);

        if (number != null)
            number.ShowHealing(amount);
    }

    private void SpawnMessage(
        Vector3 position,
        string message,
        Color color)
    {
        DamageNumber number = Spawn(position);

        if (number != null)
            number.ShowMessage(message, color);
    }

    private DamageNumber Spawn(Vector3 position)
    {
        if (damageNumberPrefab == null)
            return null;

        ProjectilePoolProvider provider =
            poolProvider != null
                ? poolProvider
                : ProjectilePoolProvider.Instance;

        GameObject numberObject = provider != null
            ? provider.Spawn(
                damageNumberPrefab,
                position + spawnOffset,
                Quaternion.identity
            )
            : Instantiate(
                damageNumberPrefab,
                position + spawnOffset,
                Quaternion.identity
            );

        return numberObject != null
            ? numberObject.GetComponent<DamageNumber>()
            : null;
    }
}
