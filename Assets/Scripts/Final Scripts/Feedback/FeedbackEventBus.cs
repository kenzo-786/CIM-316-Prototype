using System;
using UnityEngine;

public static class FeedbackEventBus 
{
    public static event Action<Vector3, float, bool> OnDamageNumberRequested;
    public static event Action<Vector3, float, float> OnScreenShakeRequested;
    public static event Action<string, Vector3> OnSoundRequested;
    public static event Action<GameObject, Vector3> OnEffectRequested;

    public static void ReportDamage(Vector3 position, float amount, bool critical = false)
    {
        OnDamageNumberRequested?.Invoke(position, amount, critical);
    }

    public static void RequestScreenShake(float intensity, float duration, Vector3 sourcePosition)
    {
        OnScreenShakeRequested?.Invoke(sourcePosition, intensity, duration);
    }

    public static void PlaySound(string soundId, Vector3 position)
    {
        if (!string.IsNullOrWhiteSpace(soundId))
            OnSoundRequested?.Invoke(soundId, position);
    }

    public static void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
            OnEffectRequested?.Invoke(effectPrefab, position);
    }
}
