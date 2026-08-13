using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHitFeedback : MonoBehaviour
{
    [Header("Local Feedback")]
    [SerializeField] private HitFlash hitFlash;

    [Header("Global Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private string hitSoundId = "enemy_hit";
    [SerializeField] private bool showDamageNumber = true;

    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake;
    [SerializeField, Min(0f)] private float shakeIntensity = 0.02f;
    [SerializeField, Min(0f)] private float shakeDuration = 0.03f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (hitFlash == null)
            hitFlash = GetComponent<HitFlash>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged(DamageInfo damageInfo)
    {
        Vector3 hitPosition = damageInfo.hitPoint;

        if (hitFlash != null)
            hitFlash.Play();

        if (showDamageNumber)
        {
            FeedbackEventBus.ReportDamage(
                hitPosition,
                damageInfo.damage,
                damageInfo.damageType
            );
        }

        FeedbackEventBus.SpawnEffect(hitEffectPrefab, hitPosition);
        FeedbackEventBus.PlaySound(hitSoundId, hitPosition);

        if (enableCameraShake &&
            shakeIntensity > 0f &&
            shakeDuration > 0f)
        {
            FeedbackEventBus.RequestScreenShake(
                shakeIntensity,
                shakeDuration,
                hitPosition
            );
        }
    }
}
