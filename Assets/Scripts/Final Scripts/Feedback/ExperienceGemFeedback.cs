using UnityEngine;

public class ExperienceGemFeedback : MonoBehaviour , IPoolable
{
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private string spawnSoundId;

    public void OnSpawnedFromPool()
    {
        FeedbackEventBus.SpawnEffect(spawnEffectPrefab, transform.position);
        FeedbackEventBus.PlaySound(spawnSoundId, transform.position);
    }

    public void OnReturnedToPool()
    {

    }
}
