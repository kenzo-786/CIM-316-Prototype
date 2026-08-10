using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossGroundHazard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private LayerMask playerLayer;

    [SerializeField]
    private Color warningColor =
        new Color(1f, 0.2f, 0.2f, 0.25f);

    [SerializeField]
    private Color activeColor =
        new Color(1f, 0f, 0f, 0.8f);

    private readonly Dictionary<IDamageable, float>
        nextDamageTimes =
            new Dictionary<IDamageable, float>();

    private GameObject owner;
    private float telegraphDuration;
    private float activeDuration;
    private float damagePerTick;
    private float tickInterval;
    private bool active;
    private Coroutine lifetimeRoutine;

    private void Awake()
    {
        if (visual == null)
        {
            visual = GetComponentInChildren<SpriteRenderer>();
        }

        if (damageCollider == null)
        {
            damageCollider = GetComponent<Collider2D>();
        }

        damageCollider.isTrigger = true;
        damageCollider.enabled = false;

        if (visual != null)
        {
            visual.color = warningColor;
        }
    }

    public void Initialize(
        GameObject hazardOwner,
        float warningTime,
        float activeTime,
        float tickDamage,
        float damageInterval)
    {
        owner = hazardOwner;
        telegraphDuration = Mathf.Max(0.1f, warningTime);
        activeDuration = Mathf.Max(0.1f, activeTime);
        damagePerTick = Mathf.Max(0f, tickDamage);
        tickInterval = Mathf.Max(0.1f, damageInterval);

        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
        }

        lifetimeRoutine =
            StartCoroutine(HazardRoutine());
    }

    private IEnumerator HazardRoutine()
    {
        active = false;
        damageCollider.enabled = false;

        float elapsed = 0f;

        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / telegraphDuration
            );

            if (visual != null)
            {
                visual.color = Color.Lerp(
                    warningColor,
                    activeColor,
                    progress
                );
            }

            yield return null;
        }

        active = true;
        damageCollider.enabled = true;

        if (visual != null)
        {
            visual.color = activeColor;
        }

        yield return new WaitForSeconds(activeDuration);

        active = false;
        damageCollider.enabled = false;
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!active || !IsInPlayerLayer(other.gameObject.layer))
        {
            return;
        }

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        if (nextDamageTimes.TryGetValue(
                damageable,
                out float nextDamageTime) &&
            Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTimes[damageable] =
            Time.time + tickInterval;

        damageable.TakeDamage(
            new DamageInfo(
                damagePerTick,
                owner != null ? owner : gameObject,
                other.ClosestPoint(transform.position)
            )
        );
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            nextDamageTimes.Remove(damageable);
        }
    }

    private bool IsInPlayerLayer(int layer)
    {
        return
            (playerLayer.value & (1 << layer)) != 0;
    }
}
