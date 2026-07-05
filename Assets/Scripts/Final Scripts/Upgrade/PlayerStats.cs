using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float critMultiplier = 2f;
    [SerializeField] private float headshotMultiplier = 3f;
    [SerializeField] private float lowHealthThreshold = 0.35f;

    private Health health;

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

    private bool rageActive;
    private float rageDamageMultiplier = 1f;
    private float rageAttackSpeedMultiplier = 1f;
    private float rageMoveSpeedMultiplier = 1f;

    private PlayerMovement movement;
    private float baseMoveSpeed;

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();

        if (movement != null)
            baseMoveSpeed = movement.MoveSpeed;
    }

    public float RollDamage(float baseDamage)
    {
        if (Random.value < OneShotChance)
            return 999999f;

        float finalDamage = baseDamage * GetDamageMultiplier();

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

        return multiplier;
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

    public void AddDamage(float value) => DamageMultiplier += value;
    public void AddAttackSpeed(float value) => AttackSpeedMultiplier += value;
    public void AddCritChance(float value) => CritChance += value;
    public void AddHeadshotChance(float value) => HeadshotChance += value;
    public void AddOneShotChance(float value) => OneShotChance += value;
    public void AddLowHealthDamage(float value) => LowHealthDamageBonus += value;
    public void AddLowHealthAttackSpeed(float value) => LowHealthAttackSpeedBonus += value;
    public void AddSideProjectiles(int value) => SideProjectiles += value;
    public void AddBackProjectiles(int value) => BackProjectiles += value;
    public void AddPierce(int value) => PierceCount += value;
    public void AddEnemyBounce(int value) => EnemyBounceCount += value;
    public void AddWallBounce(int value) => WallBounceCount += value;
    public void AddExtraLife(int value) => ExtraLives += value;
    public void AddHealOnKill(float value) => HealOnKillPercent += value;

    public bool TryUseExtraLife()
    {
        if (ExtraLives <= 0) return false;

        ExtraLives--;
        return true;
    }

    public void ActivateRage(float duration, float damageBoost, float attackSpeedBoost, float moveSpeedBoost)
    {
        StopAllCoroutines();
        StartCoroutine(RageRoutine(duration, damageBoost, attackSpeedBoost, moveSpeedBoost));
    }

    private System.Collections.IEnumerator RageRoutine(float duration, float damageBoost, float attackSpeedBoost, float moveSpeedBoost)
    {
        rageActive = true;
        rageDamageMultiplier = damageBoost;
        rageAttackSpeedMultiplier = attackSpeedBoost;
        rageMoveSpeedMultiplier = moveSpeedBoost;

        if (movement != null)
            movement.SetMoveSpeed(baseMoveSpeed * rageMoveSpeedMultiplier);

        yield return new WaitForSeconds(duration);

        rageActive = false;

        if (movement != null)
            movement.SetMoveSpeed(baseMoveSpeed);
    }

    private bool IsLowHealth()
    {
        if (health == null || health.MaxHealth <= 0) return false;
        return health.CurrentHealth / health.MaxHealth <= lowHealthThreshold;
    }
}
