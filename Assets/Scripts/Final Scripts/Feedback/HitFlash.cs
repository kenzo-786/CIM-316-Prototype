using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;

    private Color[] originalColors;
    private Coroutine routine;

    private void Awake()
    {
        CacheRenderers();
        CaptureOriginalColors();
    }

    private void OnEnable()
    {
        CacheRenderers();
        CaptureOriginalColors();
        RestoreOriginalColors();
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        RestoreOriginalColors();
    }

    public void Play()
    {
        CacheRenderers();

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
                renderer.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreOriginalColors();
        routine = null;
    }

    private void CacheRenderers()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void CaptureOriginalColors()
    {
        if (renderers == null)
            return;

        if (originalColors == null || originalColors.Length != renderers.Length)
            originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;
        }
    }

    private void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalColors.Length)
                renderers[i].color = originalColors[i];
        }
    }
}
