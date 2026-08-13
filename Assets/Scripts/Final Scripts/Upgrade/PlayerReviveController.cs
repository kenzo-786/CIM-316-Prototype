using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerReviveController : MonoBehaviour
{
    [Header("Revive")]
    [SerializeField, Range(0.05f, 1f)] private float restoredHealthPercent = 0.5f;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 2f;

    [Header("Slow Motion")]
    [SerializeField, Range(0.05f, 1f)] private float slowMotionScale = 0.25f;
    [SerializeField, Min(0f)] private float slowMotionDuration = 0.35f;

    [Header("Feedback")]
    [SerializeField] private GameObject revivalEffectPrefab;
    [SerializeField] private string revivalSoundId = "player_revive";
    [SerializeField] private Color revivalTextColor = new Color(0.3f, 1f, 0.7f, 1f);
    [SerializeField, Min(0f)] private float shakeIntensity = 0.18f;
    [SerializeField, Min(0f)] private float shakeDuration = 0.12f;

    private Health health;
    private PlayerStats stats;
    private PlayerMovement movement;
    private PlayerWeaponController weaponController;
    private PlayerDeathAnimation deathAnimation;
    private Coroutine slowMotionRoutine;
    private float previousTimeScale = 1f;
    private bool slowMotionActive;

    private void Awake()
    {
        health = GetComponent<Health>();
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
        weaponController = GetComponent<PlayerWeaponController>();
        deathAnimation = GetComponent<PlayerDeathAnimation>();
    }

    private void OnEnable()
    {
        health.OnDied += TryRevive;
    }

    private void OnDisable()
    {
        health.OnDied -= TryRevive;

        if (slowMotionRoutine != null)
            StopCoroutine(slowMotionRoutine);

        if (slowMotionActive)
            Time.timeScale = previousTimeScale;

        slowMotionRoutine = null;
        slowMotionActive = false;
    }

    private void TryRevive()
    {
        if (!stats.TryUseExtraLife())
            return;

        health.Revive(health.MaxHealth * restoredHealthPercent);
        deathAnimation?.CancelDeathAnimationForRevive();
        health.SetInvulnerable(invulnerabilityDuration);

        if (movement != null)
            movement.enabled = true;

        if (weaponController != null)
            weaponController.SetCombatActive(true);

        Vector3 position = transform.position;

        FeedbackEventBus.SpawnEffect(revivalEffectPrefab, position);
        FeedbackEventBus.PlaySound(revivalSoundId, position);
        FeedbackEventBus.ReportWorldText(position, "REVIVED", revivalTextColor);
        FeedbackEventBus.RequestScreenShake(shakeIntensity, shakeDuration, position);

        if (slowMotionRoutine != null)
            StopCoroutine(slowMotionRoutine);

        slowMotionRoutine = StartCoroutine(SlowMotionRoutine());
    }

    private IEnumerator SlowMotionRoutine()
    {
        previousTimeScale = Time.timeScale;
        slowMotionActive = true;
        Time.timeScale = slowMotionScale;

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        if (Time.timeScale == slowMotionScale)
            Time.timeScale = previousTimeScale;

        slowMotionActive = false;
        slowMotionRoutine = null;
    }
}
