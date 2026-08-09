using UnityEngine;

public class WeaponFireFeedback : MonoBehaviour
{
    [Header("Required")]
    [SerializeField]
    private PlayerWeaponBase weapon;

    [SerializeField]
    private Transform effectOrigin;

    [Header("Launch Feedback")]
    [SerializeField]
    private GameObject launchEffectPrefab;

    [SerializeField]
    private string launchSoundId;

    [Header("Optional Camera Shake")]
    [SerializeField, Min(0f)]
    private float shakeIntensity;

    [SerializeField, Min(0f)]
    private float shakeDuration;

    private void Awake()
    {
        if (weapon == null)
        {
            weapon =
                GetComponent<PlayerWeaponBase>();
        }
    }

    private void OnEnable()
    {
        if (weapon != null)
        {
            weapon.OnAttackPerformed +=
                Play;
        }
    }

    private void OnDisable()
    {
        if (weapon != null)
        {
            weapon.OnAttackPerformed -=
                Play;
        }
    }

    private void Play(
        Vector2 attackDirection)
    {
        Vector3 position =
            effectOrigin != null
                ? effectOrigin.position
                : weapon.transform.position;

        FeedbackEventBus.SpawnEffect(
            launchEffectPrefab,
            position);

        FeedbackEventBus.PlaySound(
            launchSoundId,
            position);

        if (shakeIntensity > 0f &&
            shakeDuration > 0f)
        {
            FeedbackEventBus
                .RequestScreenShake(
                    shakeIntensity,
                    shakeDuration,
                    position);
        }
    }
}
