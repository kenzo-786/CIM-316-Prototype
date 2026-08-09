using System.Collections;
using UnityEngine;
public class CameraShake : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private RoomCameraController
        roomCameraController;

    [Header("Shake Limits")]
    [SerializeField, Min(0f)]
    private float maxOffset = 0.2f;

    [SerializeField, Min(0f)]
    private float maximumDuration = 0.3f;

    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (roomCameraController == null &&
            Camera.main != null)
        {
            roomCameraController =
                Camera.main.GetComponent
                    <RoomCameraController>();
        }
    }

    private void OnEnable()
    {
        FeedbackEventBus
            .OnScreenShakeRequested +=
            HandleShakeRequested;
    }

    private void OnDisable()
    {
        FeedbackEventBus
            .OnScreenShakeRequested -=
            HandleShakeRequested;

        StopCurrentShake();
    }

    private void HandleShakeRequested(
        Vector3 sourcePosition,
        float intensity,
        float duration)
    {
        if (roomCameraController == null)
            return;

        intensity =
            Mathf.Clamp01(intensity);

        duration =
            Mathf.Clamp(
                duration,
                0f,
                maximumDuration);

        if (intensity <= 0f ||
            duration <= 0f)
        {
            return;
        }

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine =
            StartCoroutine(
                ShakeRoutine(
                    intensity,
                    duration));
    }

    private IEnumerator ShakeRoutine(
        float intensity,
        float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / duration);

            float falloff =
                1f - progress;

            Vector2 offset =
                Random.insideUnitCircle *
                maxOffset *
                intensity *
                falloff;

            roomCameraController
                .SetShakeOffset(offset);

            yield return null;
        }

        roomCameraController
            .ClearShakeOffset();

        shakeRoutine = null;
    }

    private void StopCurrentShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        if (roomCameraController != null)
        {
            roomCameraController
                .ClearShakeOffset();
        }
    }

}
