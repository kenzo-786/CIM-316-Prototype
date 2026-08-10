using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossHealthTarget : MonoBehaviour
{
    [SerializeField] private string bossName = "The Web Keeper";
    [SerializeField] private BossHealthHud healthHud;

    private Health health;
    private bool deathSent;

    public event Action<float, float> OnBossHealthChanged;
    public event Action OnBossDefeated;

    public float CurrentHealth =>
        health != null ? health.CurrentHealth : 0f;

    public float MaxHealth =>
        health != null ? health.MaxHealth : 0f;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        deathSent = false;

        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;
        }
    }

    private void Start()
    {
        if (healthHud == null)
        {
            healthHud = FindObjectOfType<BossHealthHud>();
        }

        if (healthHud != null)
        {
            healthHud.Show(this, bossName);
        }

        HandleHealthChanged(
            CurrentHealth,
            MaxHealth
        );
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
        }

        if (healthHud != null)
        {
            healthHud.Hide();
        }
    }

    private void HandleHealthChanged(
        float currentHealth,
        float maximumHealth)
    {
        OnBossHealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );
    }

    private void HandleDied()
    {
        if (deathSent)
        {
            return;
        }

        deathSent = true;
        OnBossDefeated?.Invoke();
    }
}

