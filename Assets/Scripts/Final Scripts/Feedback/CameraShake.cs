using System.Collections;
using UnityEngine;
public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxOffset = 0.25f;

    private Vector3 originalLocalPosition;
    private Coroutine routine;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
            originalLocalPosition = cameraTransform.localPosition;
    }

    private void OnEnable()
    {
        FeedbackEventBus.OnScreenShakeRequested += Shake;
    }

    private void OnDisable()
    {
        FeedbackEventBus.OnScreenShakeRequested -= Shake;
    }

    private void Shake(Vector3 sourcePosition, float intensity, float duration)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            Vector2 offset = Random.insideUnitCircle * maxOffset * intensity;
            cameraTransform.localPosition = originalLocalPosition + (Vector3)offset;
            yield return null;
        }

        cameraTransform.localPosition = originalLocalPosition;
    }
}
