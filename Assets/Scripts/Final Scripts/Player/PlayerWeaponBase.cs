using UnityEngine;

public abstract class PlayerWeaponBase : MonoBehaviour
{
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float baseCooldown = 0.5f;

    private float nextAttackTime;
    protected PlayerStats stats;

    protected virtual void Awake()
    {
        stats = GetComponentInParent<PlayerStats>();
    }

    public bool TryAttack(Vector2 aimDirection)
    {
        float attackSpeed = stats != null ? stats.GetAttackSpeedMultiplier() : 1f;
        float cooldown = baseCooldown / attackSpeed;

        if (Time.time < nextAttackTime) return false;

        nextAttackTime = Time.time + cooldown;
        Attack(aimDirection.normalized);
        return true;
    }

    protected float GetFinalDamage()
    {
        if (stats == null) return baseDamage;
        return stats.RollDamage(baseDamage);
    }

    protected abstract void Attack(Vector2 aimDirection);
}
