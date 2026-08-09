using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float maxHealth = 100f;

    public event Action<float, float>
        OnHealthChanged;

    public event Action<DamageInfo>
        OnDamaged;

    public event Action<float>
        OnHealed;

    public event Action<bool>
        OnInvulnerabilityChanged;

    public event Action OnDied;

    public float CurrentHealth
    {
        get;
        private set;
    }

    public float MaxHealth =>
        maxHealth;

    public bool IsDead
    {
        get;
        private set;
    }

    public bool IsInvulnerable
    {
        get;
        private set;
    }

    private Coroutine
        invulnerabilityRoutine;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        OnHealthChanged?.Invoke(
            CurrentHealth,
            maxHealth);
    }

    public void SetMaxHealth(
        float value,
        bool refill)
    {
        maxHealth =
            Mathf.Max(
                1f,
                value);

        if (refill)
        {
            CurrentHealth =
                maxHealth;

            IsDead = false;
        }
        else
        {
            CurrentHealth =
                Mathf.Min(
                    CurrentHealth,
                    maxHealth);
        }

        OnHealthChanged?.Invoke(
            CurrentHealth,
            maxHealth);
    }

    public void TakeDamage(
       DamageInfo damageInfo)
    {
        if (IsDead ||
           IsInvulnerable ||
           damageInfo.damage <= 0f)
        {
            return;
        }

        CurrentHealth =
            Mathf.Max(
                0f,
                CurrentHealth -
                damageInfo.damage);

        OnHealthChanged?.Invoke(
            CurrentHealth,
            maxHealth);

        OnDamaged?.Invoke(
        damageInfo);

        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(
            new DamageInfo(
                amount,
                null,
                transform.position));
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        float previousHealth =
            CurrentHealth;

        CurrentHealth =
            Mathf.Min(
                maxHealth,
                CurrentHealth + amount);

        float actualHealing =
            CurrentHealth -
            previousHealth;

        if (actualHealing <= 0f)
            return;

        OnHealthChanged?.Invoke(
            CurrentHealth,
            maxHealth);

        OnHealed?.Invoke(
            actualHealing);
    }

    public void Revive(
        float healthAmount)
    {
        IsDead = false;

        CurrentHealth =
            Mathf.Clamp(
                healthAmount,
                1f,
                maxHealth);

        OnHealthChanged?.Invoke(
            CurrentHealth,
            maxHealth);
    }

    public void SetInvulnerable(
        float duration)
    {
        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(
                invulnerabilityRoutine);
        }

        invulnerabilityRoutine =
            StartCoroutine(
                InvulnerabilityRoutine(
                    duration));
    }

    private IEnumerator
        InvulnerabilityRoutine(
            float duration)
    {
        IsInvulnerable = true;

        OnInvulnerabilityChanged?.Invoke(
            true);

        yield return
            new WaitForSecondsRealtime(
                Mathf.Max(
                    0f,
                    duration));

        IsInvulnerable = false;

        OnInvulnerabilityChanged?.Invoke(
            false);

        invulnerabilityRoutine = null;
    }
}
