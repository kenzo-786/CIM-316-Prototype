using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Pop")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScale(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScale(originalScale);
    }

    private void StartScale(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(
            SmoothScale(targetScale)
        );
    }

    private IEnumerator SmoothScale(Vector3 targetScale)
    {
        Vector3 startingScale = transform.localScale;
        float time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                startingScale,
                targetScale,
                time / popSpeed
            );

            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}