using System;
using UnityEngine;

public class BossHealthTarget : MonoBehaviour
{
    private Health health;
    private float lastCurrentHealth;
    private float lastMaxHealth;
    private bool deathSent;

    public event Action<float, float> OnBossHealthChanged;
    public event Action OnBossDefeated;

    public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
    public float MaxHealth => health != null ? health.MaxHealth : 0f;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        deathSent = false;
        CacheAndNotify();
    }

    private void Update()
    {
        if (health == null)
            return;

        if (!Mathf.Approximately(lastCurrentHealth, health.CurrentHealth) ||
            !Mathf.Approximately(lastMaxHealth, health.MaxHealth))
        {
            CacheAndNotify();
        }

        if (!deathSent && health.CurrentHealth <= 0f)
        {
            deathSent = true;
            OnBossDefeated?.Invoke();
        }
    }

    private void CacheAndNotify()
    {
        if (health == null)
            return;

        lastCurrentHealth = health.CurrentHealth;
        lastMaxHealth = health.MaxHealth;
        OnBossHealthChanged?.Invoke(lastCurrentHealth, lastMaxHealth);
    }
}
