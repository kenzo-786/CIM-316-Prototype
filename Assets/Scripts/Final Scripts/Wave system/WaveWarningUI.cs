using System.Collections;
using TMPro;
using UnityEngine;

public class WaveWarningUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomCombatController combatController;
    [SerializeField] private CanvasGroup warningCanvasGroup;
    [SerializeField] private RectTransform warningTransform;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.18f;
    [SerializeField, Range(0.1f, 1f)] private float startingScale = 0.8f;

    private Coroutine animationRoutine;
    private Vector3 normalScale = Vector3.one;

    private void Awake()
    {
        if (combatController == null)
        {
            combatController =
                FindObjectOfType<RoomCombatController>();
        }

        if (warningTransform != null)
        {
            normalScale = warningTransform.localScale;
        }

        HideImmediate();
    }

    private void OnEnable()
    {
        if (combatController == null)
        {
            combatController =
                FindObjectOfType<RoomCombatController>();
        }

        if (combatController != null)
        {
            combatController.OnWaveWarning += ShowWarning;
            combatController.OnWaveStarted += HandleWaveStarted;
            combatController.OnRoomCombatCleared += HideImmediate;
            combatController.OnRoomCombatStopped += HideImmediate;
        }

        HideImmediate();
    }

    private void OnDisable()
    {
        if (combatController != null)
        {
            combatController.OnWaveWarning -= ShowWarning;
            combatController.OnWaveStarted -= HandleWaveStarted;
            combatController.OnRoomCombatCleared -= HideImmediate;
            combatController.OnRoomCombatStopped -= HideImmediate;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        HideImmediate();
    }

    private void ShowWarning(
        int waveNumber,
        int totalWaves,
        float duration)
    {
        if (warningCanvasGroup == null ||
            warningTransform == null)
        {
            return;
        }

        bool finalWave = waveNumber == totalWaves;

        if (titleText != null)
        {
            titleText.text =
                finalWave
                    ? "FINAL WAVE"
                    : $"WAVE {waveNumber}";
        }

        if (subtitleText != null)
        {
            subtitleText.text = "INCOMING";
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(
            AnimateWarning(Mathf.Max(0.1f, duration))
        );
    }

    private void HandleWaveStarted(
        int waveNumber,
        int totalWaves)
    {
        HideImmediate();
    }

    private IEnumerator AnimateWarning(float duration)
    {
        warningCanvasGroup.alpha = 0f;

        warningTransform.localScale =
            normalScale * startingScale;

        float fadeInDuration =
            Mathf.Min(fadeDuration, duration * 0.35f);

        float fadeOutDuration =
            Mathf.Min(fadeDuration, duration * 0.35f);

        float holdDuration =
            Mathf.Max(
                0f,
                duration - fadeInDuration - fadeOutDuration
            );

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;

            float progress =
                fadeInDuration > 0f
                    ? Mathf.Clamp01(timer / fadeInDuration)
                    : 1f;

            warningCanvasGroup.alpha = progress;

            warningTransform.localScale =
                Vector3.Lerp(
                    normalScale * startingScale,
                    normalScale,
                    progress
                );

            yield return null;
        }

        warningCanvasGroup.alpha = 1f;
        warningTransform.localScale = normalScale;

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;

            float progress =
                fadeOutDuration > 0f
                    ? Mathf.Clamp01(timer / fadeOutDuration)
                    : 1f;

            warningCanvasGroup.alpha = 1f - progress;

            yield return null;
        }

        HideImmediate();
        animationRoutine = null;
    }

    private void HideImmediate()
    {
        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
            warningCanvasGroup.interactable = false;
            warningCanvasGroup.blocksRaycasts = false;
        }

        if (warningTransform != null)
        {
            warningTransform.localScale = normalScale;
        }
    }
}
