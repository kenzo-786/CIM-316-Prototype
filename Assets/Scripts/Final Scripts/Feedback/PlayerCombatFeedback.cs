using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Health))]
public class PlayerCombatFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer[] playerRenderers;
    [SerializeField] private CanvasGroup damageVignette;
    [SerializeField] private CanvasGroup lowHealthVignette;
    [SerializeField] private RectTransform directionalIndicator;
    [SerializeField] private CanvasGroup directionalIndicatorCanvas;

    [Header("Damage")]
    [SerializeField]
    private Color damageFlashColor = new Color(1f, 0.2f, 0.2f, 1f);

    [SerializeField, Min(0f)] private float damageFlashDuration = 0.1f;
    [SerializeField, Min(0f)] private float hitInvulnerabilityDuration = 0.45f;
    [SerializeField, Min(0f)] private float blinkInterval = 0.08f;
    [SerializeField] private string damageSoundId = "player_hurt";
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private bool enableDamageShake = true;
    [SerializeField, Min(0f)] private float damageShakeIntensity = 0.06f;
    [SerializeField, Min(0f)] private float damageShakeDuration = 0.06f;

    [Header("Healing")]
    [SerializeField] private GameObject healingEffectPrefab;
    [SerializeField] private string healingSoundId = "player_heal";

    [Header("Low Health")]
    [SerializeField, Range(0.05f, 0.95f)]
    private float lowHealthThreshold = 0.3f;

    [SerializeField, Range(0f, 1f)]
    private float lowHealthVignetteAlpha = 0.2f;

    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField, Range(0f, 1f)]
    private float lowHealthMusicMultiplier = 0.75f;

    private Health health;
    private Color[] normalColors;
    private Coroutine flashRoutine;
    private Coroutine vignetteRoutine;
    private Coroutine indicatorRoutine;
    private Coroutine blinkRoutine;
    private bool lowHealth;
    private float normalMusicVolume = 1f;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        normalColors = new Color[playerRenderers.Length];

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                normalColors[i] = playerRenderers[i].color;
            }
        }

        if (musicSource != null)
        {
            normalMusicVolume = musicSource.volume;
        }

        SetCanvasAlpha(damageVignette, 0f);
        SetCanvasAlpha(lowHealthVignette, 0f);
        SetCanvasAlpha(directionalIndicatorCanvas, 0f);
    }

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
        health.OnHealed += HandleHealed;
        health.OnHealthChanged += HandleHealthChanged;
        health.OnInvulnerabilityChanged += HandleInvulnerabilityChanged;

        HandleHealthChanged(
            health.CurrentHealth,
            health.MaxHealth
        );
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnHealed -= HandleHealed;
        health.OnHealthChanged -= HandleHealthChanged;
        health.OnInvulnerabilityChanged -= HandleInvulnerabilityChanged;

        RestoreRenderers();
        SetLowHealthAudio(false);

        SetCanvasAlpha(damageVignette, 0f);
        SetCanvasAlpha(lowHealthVignette, 0f);
        SetCanvasAlpha(directionalIndicatorCanvas, 0f);

        if (musicSource != null)
        {
            musicSource.volume = normalMusicVolume;
        }
    }

    private void Update()
    {
        float targetAlpha = lowHealth
            ? lowHealthVignetteAlpha
            : 0f;

        if (lowHealthVignette != null)
        {
            lowHealthVignette.alpha = Mathf.MoveTowards(
                lowHealthVignette.alpha,
                targetAlpha,
                Time.unscaledDeltaTime * 1.5f
            );
        }

        if (musicSource != null)
        {
            float targetVolume = lowHealth
                ? normalMusicVolume * lowHealthMusicMultiplier
                : normalMusicVolume;

            musicSource.volume = Mathf.MoveTowards(
                musicSource.volume,
                targetVolume,
                Time.unscaledDeltaTime
            );
        }
    }

    private void HandleDamaged(DamageInfo damageInfo)
    {
        Vector3 position = transform.position;

        FeedbackEventBus.PlaySound(
            damageSoundId,
            position
        );

        FeedbackEventBus.SpawnEffect(
            damageEffectPrefab,
            position
        );

        if (enableDamageShake)
        {
            FeedbackEventBus.RequestScreenShake(
                damageShakeIntensity,
                damageShakeDuration,
                position
            );
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(
            DamageFlashRoutine()
        );

        if (vignetteRoutine != null)
        {
            StopCoroutine(vignetteRoutine);
        }

        vignetteRoutine = StartCoroutine(
            DamageVignetteRoutine()
        );

        ShowDirectionalIndicator(
            damageInfo.source
        );

        if (health.CurrentHealth > 0f)
        {
            health.SetInvulnerable(
                hitInvulnerabilityDuration
            );
        }
    }

    private void HandleHealed(float amount)
    {
        FeedbackEventBus.ReportHealing(
            transform.position,
            amount
        );

        FeedbackEventBus.SpawnEffect(
            healingEffectPrefab,
            transform.position
        );

        FeedbackEventBus.PlaySound(
            healingSoundId,
            transform.position
        );
    }

    private void HandleHealthChanged(
        float current,
        float maximum
    )
    {
        lowHealth =
            !health.IsDead &&
            maximum > 0f &&
            current / maximum <= lowHealthThreshold;

        SetLowHealthAudio(lowHealth);
    }

    private void HandleInvulnerabilityChanged(
        bool active
    )
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        if (active)
        {
            blinkRoutine = StartCoroutine(
                BlinkRoutine()
            );
        }
        else
        {
            RestoreRendererVisibility();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        SetRendererColors(damageFlashColor);

        yield return new WaitForSecondsRealtime(
            damageFlashDuration
        );

        RestoreRendererColors();
        flashRoutine = null;
    }

    private IEnumerator DamageVignetteRoutine()
    {
        if (damageVignette == null)
        {
            yield break;
        }

        damageVignette.alpha = 0.45f;

        while (damageVignette.alpha > 0f)
        {
            damageVignette.alpha = Mathf.MoveTowards(
                damageVignette.alpha,
                0f,
                Time.unscaledDeltaTime * 2.5f
            );

            yield return null;
        }

        vignetteRoutine = null;
    }

    private void ShowDirectionalIndicator(
        GameObject source
    )
    {
        if (directionalIndicator == null ||
            directionalIndicatorCanvas == null)
        {
            return;
        }

        Vector2 direction = source != null
            ? ((Vector2)source.transform.position -
               (Vector2)transform.position).normalized
            : Vector2.up;

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        directionalIndicator.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle - 90f
            );

        if (indicatorRoutine != null)
        {
            StopCoroutine(indicatorRoutine);
        }

        indicatorRoutine = StartCoroutine(
            IndicatorRoutine()
        );
    }

    private IEnumerator IndicatorRoutine()
    {
        directionalIndicatorCanvas.alpha = 1f;

        yield return new WaitForSecondsRealtime(0.2f);

        while (directionalIndicatorCanvas.alpha > 0f)
        {
            directionalIndicatorCanvas.alpha =
                Mathf.MoveTowards(
                    directionalIndicatorCanvas.alpha,
                    0f,
                    Time.unscaledDeltaTime * 4f
                );

            yield return null;
        }

        indicatorRoutine = null;
    }

    private IEnumerator BlinkRoutine()
    {
        while (health.IsInvulnerable)
        {
            SetRendererVisibility(false);

            yield return new WaitForSecondsRealtime(
                blinkInterval
            );

            SetRendererVisibility(true);

            yield return new WaitForSecondsRealtime(
                blinkInterval
            );
        }

        RestoreRendererVisibility();
        blinkRoutine = null;
    }

    private void SetLowHealthAudio(bool active)
    {
        if (heartbeatSource == null)
        {
            return;
        }

        if (active && !heartbeatSource.isPlaying)
        {
            heartbeatSource.Play();
        }
        else if (!active && heartbeatSource.isPlaying)
        {
            heartbeatSource.Stop();
        }
    }

    private void SetRendererColors(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in playerRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }

    private void RestoreRendererColors()
    {
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].color =
                    normalColors[i];
            }
        }
    }

    private void SetRendererVisibility(bool visible)
    {
        foreach (SpriteRenderer spriteRenderer in playerRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }

    private void RestoreRendererVisibility()
    {
        SetRendererVisibility(true);
    }

    private void RestoreRenderers()
    {
        RestoreRendererColors();
        RestoreRendererVisibility();
    }

    private static void SetCanvasAlpha(
        CanvasGroup group,
        float alpha
    )
    {
        if (group != null)
        {
            group.alpha = alpha;
        }
    }
}
