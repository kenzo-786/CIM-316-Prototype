using System;
using UnityEngine;

public abstract class PlayerWeaponBase : MonoBehaviour
{
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float baseCooldown = 0.5f;

    private float nextAttackTime;
    private Transform attackTarget;

    protected PlayerStats stats;
    protected Transform AttackTarget => attackTarget;

    public event Action<Vector2> OnAttackPerformed;

    protected virtual void Awake()
    {
        stats = GetComponentInParent<PlayerStats>();
    }

    public bool TryAttack(Vector2 aimDirection)
    {
        float attackSpeed = stats != null
           ? stats.GetAttackSpeedMultiplier()
           : 1f;

        float cooldown =
            baseCooldown /
            Mathf.Max(0.01f, attackSpeed);

        if (Time.time < nextAttackTime)
            return false;

        if (aimDirection == Vector2.zero)
            aimDirection = Vector2.right;

        Vector2 normalizedDirection =
            aimDirection.normalized;

        nextAttackTime =
            Time.time + cooldown;

        Attack(normalizedDirection);

        OnAttackPerformed?.Invoke(
            normalizedDirection);

        return true;
    }

    public void SetAttackTarget(Transform target)
    {
        attackTarget = target;
    }

    protected float GetFinalDamage()
    {
        if (stats == null)
            return baseDamage;

        return stats.RollDamage(baseDamage);
    }

    protected abstract void Attack(Vector2 aimDirection);
}
