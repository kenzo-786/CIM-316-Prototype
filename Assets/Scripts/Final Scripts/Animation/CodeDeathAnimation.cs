using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class CodeDeathAnimation : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField, Min(0.05f)] private float duration = 0.35f;
    [SerializeField, Range(0f, 1f)] private float endScaleMultiplier = 0.12f;
    [SerializeField, Min(0f)] private float squashAmount = 0.12f;
    [SerializeField] private float spinDegrees = 140f;
    [SerializeField] private bool fadeSprites = true;

    private Health health;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color[] originalColors;
    private Coroutine deathRoutine;
    private bool played;

    public float Duration => duration;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (visualRoot == null)
        {
            SpriteRenderer firstRenderer = GetComponentInChildren<SpriteRenderer>();

            if (firstRenderer != null)
                visualRoot = firstRenderer.transform;
            else
                visualRoot = transform;
        }

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        originalScale = visualRoot.localScale;
        originalRotation = visualRoot.localRotation;
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;
        }
    }

    private void OnEnable()
    {
        health.OnDied += PlayDeath;
        ResetVisuals();
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= PlayDeath;
    }

    private void Update()
    {
        if (played && health != null && !health.IsDead)
            ResetVisuals();
    }

    private void PlayDeath()
    {
        if (played)
            return;

        played = true;

        if (deathRoutine != null)
            StopCoroutine(deathRoutine);

        deathRoutine = StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / duration);
            float squash = Mathf.Sin(progress * Mathf.PI) * squashAmount;
            float scaleMultiplier = Mathf.Lerp(1f, endScaleMultiplier, progress);

            Vector3 scale = originalScale * scaleMultiplier;
            scale.x *= 1f + squash;
            scale.y *= 1f - squash;

            visualRoot.localScale = scale;
            visualRoot.localRotation = originalRotation * Quaternion.Euler(0f, 0f, spinDegrees * progress);

            if (fadeSprites)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null)
                        continue;

                    Color color = originalColors[i];
                    color.a *= 1f - progress;
                    renderers[i].color = color;
                }
            }

            yield return null;
        }

        deathRoutine = null;
    }

    private void ResetVisuals()
    {
        if (deathRoutine != null)
        {
            StopCoroutine(deathRoutine);
            deathRoutine = null;
        }

        played = false;

        if (visualRoot != null)
        {
            visualRoot.localScale = originalScale;
            visualRoot.localRotation = originalRotation;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }
    }
}
