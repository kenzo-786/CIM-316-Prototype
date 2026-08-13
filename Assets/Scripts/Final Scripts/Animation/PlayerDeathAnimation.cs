using System.Collections;
using UnityEngine;
public class PlayerDeathAnimation : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField, Min(0.05f)] private float duration = 0.6f;
    [SerializeField, Range(0f, 1f)] private float endScaleMultiplier = 0.2f;
    [SerializeField, Min(0f)] private float squashAmount = 0.12f;
    [SerializeField] private float spinDegrees = 120f;
    [SerializeField] private bool fadeSprites = true;

    private Health health;
    private PlayerMovement movement;
    private PlayerWeaponController weaponController;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color[] originalColors;
    private Coroutine deathRoutine;

    public bool IsPlayingFinalDeath { get; private set; }

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        weaponController = GetComponent<PlayerWeaponController>();

        if (visualRoot == null)
        {
            SpriteRenderer firstRenderer = GetComponentInChildren<SpriteRenderer>();

            if (firstRenderer != null)
                visualRoot = firstRenderer.transform;
            else
                visualRoot = transform;
        }

        if (renderers == null || renderers.Length == 0)
            renderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);

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
        health.OnDied += HandleDeath;
        ResetVisuals();
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDeath;

        if (deathRoutine != null)
            StopCoroutine(deathRoutine);

        deathRoutine = null;
        IsPlayingFinalDeath = false;
    }

    private void HandleDeath()
    {
        if (deathRoutine != null)
            return;

        deathRoutine = StartCoroutine(DeathRoutine());
    }

    public void CancelDeathAnimationForRevive()
    {
        if (deathRoutine != null)
            StopCoroutine(deathRoutine);

        deathRoutine = null;
        ResetVisuals();
    }

    public IEnumerator WaitForFinalDeath()
    {
        while (deathRoutine != null)
            yield return null;
    }

    private IEnumerator DeathRoutine()
    {
        yield return null;

        if (health == null || !health.IsDead)
        {
            deathRoutine = null;
            ResetVisuals();
            yield break;
        }

        IsPlayingFinalDeath = true;

        if (movement != null)
            movement.enabled = false;

        if (weaponController != null)
            weaponController.SetCombatActive(false);

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
            visualRoot.localRotation = originalRotation *
                                       Quaternion.Euler(0f, 0f, spinDegrees * progress);

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
        IsPlayingFinalDeath = false;
    }

    private void ResetVisuals()
    {
        IsPlayingFinalDeath = false;

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
