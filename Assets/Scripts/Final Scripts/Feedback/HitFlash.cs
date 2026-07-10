using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;

    private Color[] originalColors;
    private Coroutine routine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    public void Play()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        foreach (SpriteRenderer renderer in renderers)
            renderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];
    }
}
