using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour
{
    [Header("Damage Rolls")]
    [SerializeField] private float critMultiplier = 2f;
    [SerializeField] private float headshotMultiplier = 3f;
    [SerializeField] private float lowHealthThreshold = 0.35f;

    private Health health;
    private PlayerMovement movement;
    private Coroutine rageRoutine;

    private bool rageActive;
    private float rageDamageMultiplier = 1f;
    private float rageAttackSpeedMultiplier = 1f;
    private float movementSpeedBeforeRage;

    public float DamageMultiplier { get; private set; } = 1f;
    public float AttackSpeedMultiplier { get; private set; } = 1f;

    public float CritChance { get; private set; }
    public float HeadshotChance { get; private set; }
    public float OneShotChance { get; private set; }

    public float LowHealthDamageBonus { get; private set; }
    public float LowHealthAttackSpeedBonus { get; private set; }

    public int SideProjectiles { get; private set; }
    public int BackProjectiles { get; private set; }
    public int PierceCount { get; private set; }
    public int EnemyBounceCount { get; private set; }
    public int WallBounceCount { get; private set; }
    public int ExtraLives { get; private set; }

    public float HealOnKillPercent { get; private set; }

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnDisable()
    {
        RestoreMovementAfterRage();
    }

    public float RollDamage(float baseDamage)
    {
        if (Random.value < OneShotChance)
            return 999999f;

        float finalDamage =
            baseDamage * GetDamageMultiplier();

        if (Random.value < HeadshotChance)
            finalDamage *= headshotMultiplier;
        else if (Random.value < CritChance)
            finalDamage *= critMultiplier;

        return finalDamage;
    }

    public float GetDamageMultiplier()
    {
        float multiplier = DamageMultiplier;

        if (IsLowHealth())
            multiplier += LowHealthDamageBonus;

        if (rageActive)
            multiplier *= rageDamageMultiplier;

        return Mathf.Max(0.1f, multiplier);
    }

    public float GetAttackSpeedMultiplier()
    {
        float multiplier = AttackSpeedMultiplier;

        if (IsLowHealth())
            multiplier += LowHealthAttackSpeedBonus;

        if (rageActive)
            multiplier *= rageAttackSpeedMultiplier;

        return Mathf.Max(0.1f, multiplier);
    }

    public void AddDamage(float value)
    {
        DamageMultiplier += Mathf.Max(0f, value);
    }

    public void AddAttackSpeed(float value)
    {
        AttackSpeedMultiplier += Mathf.Max(0f, value);
    }

    public void AddCritChance(float value)
    {
        CritChance = Mathf.Clamp01(CritChance + value);
    }

    public void AddHeadshotChance(float value)
    {
        HeadshotChance =
            Mathf.Clamp01(HeadshotChance + value);
    }

    public void AddOneShotChance(float value)
    {
        OneShotChance =
            Mathf.Clamp01(OneShotChance + value);
    }

    public void AddLowHealthDamage(float value)
    {
        LowHealthDamageBonus += Mathf.Max(0f, value);
    }

    public void AddLowHealthAttackSpeed(float value)
    {
        LowHealthAttackSpeedBonus +=
            Mathf.Max(0f, value);
    }

    public void AddSideProjectiles(int value)
    {
        SideProjectiles += Mathf.Max(0, value);
    }

    public void AddBackProjectiles(int value)
    {
        BackProjectiles += Mathf.Max(0, value);
    }

    public void AddPierce(int value)
    {
        PierceCount += Mathf.Max(0, value);
    }

    public void AddEnemyBounce(int value)
    {
        EnemyBounceCount += Mathf.Max(0, value);
    }

    public void AddWallBounce(int value)
    {
        WallBounceCount += Mathf.Max(0, value);
    }

    public void AddExtraLife(int value)
    {
        ExtraLives += Mathf.Max(0, value);
    }

    public void AddHealOnKill(float value)
    {
        HealOnKillPercent =
            Mathf.Clamp01(HealOnKillPercent + value);
    }

    public bool TryUseExtraLife()
    {
        if (ExtraLives <= 0)
            return false;

        ExtraLives--;
        return true;
    }

    public void ActivateRage(
        float duration,
        float damageBoost,
        float attackSpeedBoost,
        float moveSpeedBoost)
    {
        if (rageRoutine != null)
        {
            StopCoroutine(rageRoutine);
            rageRoutine = null;

            RestoreMovementAfterRage();
        }

        rageRoutine = StartCoroutine(
            RageRoutine(
                duration,
                damageBoost,
                attackSpeedBoost,
                moveSpeedBoost
            )
        );
    }

    private IEnumerator RageRoutine(
        float duration,
        float damageBoost,
        float attackSpeedBoost,
        float moveSpeedBoost)
    {
        rageActive = true;

        rageDamageMultiplier =
            Mathf.Max(1f, damageBoost);

        rageAttackSpeedMultiplier =
            Mathf.Max(1f, attackSpeedBoost);

        if (movement != null)
        {
            movementSpeedBeforeRage =
                movement.MoveSpeed;

            movement.SetMoveSpeed(
                movementSpeedBeforeRage *
                Mathf.Max(1f, moveSpeedBoost)
            );
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, duration)
        );

        RestoreMovementAfterRage();
        rageRoutine = null;
    }

    private void RestoreMovementAfterRage()
    {
        if (rageActive && movement != null)
        {
            movement.SetMoveSpeed(
                movementSpeedBeforeRage
            );
        }

        rageActive = false;
        rageDamageMultiplier = 1f;
        rageAttackSpeedMultiplier = 1f;
    }

    private bool IsLowHealth()
    {
        if (health == null || health.MaxHealth <= 0f)
            return false;

        return
            health.CurrentHealth / health.MaxHealth <=
            lowHealthThreshold;
    }
}
