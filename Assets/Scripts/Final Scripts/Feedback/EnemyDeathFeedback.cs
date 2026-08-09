using System.Collections;
using UnityEngine;


public class EnemyDeathFeedback : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private bool fadeSprites = true;
    [SerializeField, Min(0f)] private float deathDuration = 0.25f;

    [Header("Death Cue")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private string deathSoundId = "enemy_death";

    [Header("Heavy Enemy")]
    [SerializeField] private bool heavyDeath;
    [SerializeField] private GameObject heavyDeathEffectPrefab;
    [SerializeField] private string heavyDeathSoundId = "heavy_enemy_death";
    [SerializeField, Min(0f)] private float heavyShakeIntensity = 0.12f;
    [SerializeField, Min(0f)] private float heavyShakeDuration = 0.1f;

    [Header("Multiplier Split")]
    [SerializeField] private bool splitDeath;
    [SerializeField] private GameObject splitEffectPrefab;
    [SerializeField] private string splitSoundId = "enemy_split";

    private Color[] originalColors;
    private bool played;

    public float DeathDuration => deathDuration;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] = renderers[i].color;
            }
        }
    }

    public void PlayDeath()
    {
        if (played)
        {
            return;
        }

        played = true;
        Vector3 position = transform.position;

        FeedbackEventBus.SpawnEffect(deathEffectPrefab, position);
        FeedbackEventBus.PlaySound(deathSoundId, position);

        if (heavyDeath)
        {
            FeedbackEventBus.SpawnEffect(heavyDeathEffectPrefab, position);
            FeedbackEventBus.PlaySound(heavyDeathSoundId, position);
            FeedbackEventBus.RequestScreenShake(
                heavyShakeIntensity,
                heavyShakeDuration,
                position
            );
        }

        if (splitDeath)
        {
            FeedbackEventBus.SpawnEffect(splitEffectPrefab, position);
            FeedbackEventBus.PlaySound(splitSoundId, position);
        }

        if (fadeSprites && deathDuration > 0f)
        {
            StartCoroutine(FadeRoutine());
        }
    }

    private IEnumerator FadeRoutine()
    {
        float timer = 0f;

        while (timer < deathDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / deathDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                Color color = originalColors[i];
                color.a *= alpha;
                renderers[i].color = color;
            }

            yield return null;
        }
    }
}
