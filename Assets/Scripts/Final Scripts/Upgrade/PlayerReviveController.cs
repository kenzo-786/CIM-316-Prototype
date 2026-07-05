using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerReviveController : MonoBehaviour
{
    private Health health;
    private PlayerStats stats;
    private PlayerMovement movement;

    private void Awake()
    {
        health = GetComponent<Health>();
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        health.OnDied += TryRevive;
    }

    private void OnDisable()
    {
        health.OnDied -= TryRevive;
    }

    private void TryRevive()
    {
        if (!stats.TryUseExtraLife()) return;

        health.Revive(health.MaxHealth * 0.5f);

        if (movement != null)
            movement.enabled = true;
    }
}
