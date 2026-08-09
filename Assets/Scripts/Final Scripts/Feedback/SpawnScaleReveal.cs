using System.Collections;
using UnityEngine;

public class SpawnScaleReveal : MonoBehaviour
{
    public void Play(Vector3 finalScale, float duration)
    {
        StartCoroutine(RevealRoutine(finalScale, duration));
    }

    private IEnumerator RevealRoutine(Vector3 finalScale, float duration)
    {
        Vector3 startScale = finalScale * 0.2f;
        transform.localScale = startScale;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, duration));
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.LerpUnclamped(startScale, finalScale, t);

            yield return null;
        }

        transform.localScale = finalScale;
        Destroy(this);
    }
}
