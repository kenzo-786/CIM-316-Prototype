using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private RectTransform pulseRoot;
    [SerializeField] private bool hideWhenFull;

    [Header("Smoothing")]
    [SerializeField, Min(0.1f)]
    private float fillSmoothSpeed = 8f;

    [Header("Pulse")]
    [SerializeField, Min(0f)]
    private float hitPulseAmount = 0.12f;

    [SerializeField, Min(0f)]
    private float lowHealthPulseAmount = 0.04f;

    [SerializeField, Min(0f)]
    private float lowHealthPulseSpeed = 5f;

    [SerializeField, Range(0.05f, 0.95f)]
    private float lowHealthThreshold = 0.3f;

    private Camera mainCamera;
    private Vector3 baseScale = Vector3.one;
    private float targetFill = 1f;
    private float impactPulse;
    private bool lowHealth;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (pulseRoot == null)
        {
            pulseRoot = transform as RectTransform;
        }

        if (pulseRoot != null)
        {
            baseScale = pulseRoot.localScale;
        }
    }

    private void OnEnable()
    {
        health.OnHealthChanged += UpdateBar;
        health.OnDamaged += HandleDamaged;
        health.OnHealed += HandleHealed;
        health.OnDied += Hide;

        UpdateBar(
            health.CurrentHealth,
            health.MaxHealth
        );
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= UpdateBar;
        health.OnDamaged -= HandleDamaged;
        health.OnHealed -= HandleHealed;
        health.OnDied -= Hide;
    }

    private void Update()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.MoveTowards(
                fillImage.fillAmount,
                targetFill,
                fillSmoothSpeed * Time.unscaledDeltaTime
            );
        }

        impactPulse = Mathf.MoveTowards(
            impactPulse,
            0f,
            Time.unscaledDeltaTime * 5f
        );

        float lowPulse = lowHealth
            ? (Mathf.Sin(
                Time.unscaledTime *
                lowHealthPulseSpeed
            ) + 1f) *
              0.5f *
              lowHealthPulseAmount
            : 0f;

        if (pulseRoot != null)
        {
            pulseRoot.localScale =
                baseScale *
                (1f +
                 impactPulse * hitPulseAmount +
                 lowPulse);
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            transform.rotation =
                mainCamera.transform.rotation;
        }
    }

    private void UpdateBar(
        float current,
        float maximum
    )
    {
        targetFill = maximum > 0f
            ? current / maximum
            : 0f;

        lowHealth =
            current > 0f &&
            targetFill <= lowHealthThreshold;

        if (visualRoot != null)
        {
            visualRoot.SetActive(
                !hideWhenFull ||
                current < maximum
            );
        }
    }

    private void HandleDamaged(
        DamageInfo damageInfo
    )
    {
        impactPulse = 1f;
    }

    private void HandleHealed(float amount)
    {
        impactPulse = 0.7f;
    }

    private void Hide()
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }
    }
}
