using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Health))]
public class PlayerKillHealController : MonoBehaviour
{
    private PlayerStats stats;
    private Health health;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        EnemyEvents.OnEnemyDied += HealFromKill;
    }

    private void OnDisable()
    {
        EnemyEvents.OnEnemyDied -= HealFromKill;
    }

    private void HealFromKill(EnemyBase enemy)
    {
        if (stats.HealOnKillPercent <= 0f) return;

        float healAmount = health.MaxHealth * stats.HealOnKillPercent;
        health.Heal(healAmount);
    }
}
