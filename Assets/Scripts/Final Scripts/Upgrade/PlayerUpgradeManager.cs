using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Health))]
public class PlayerUpgradeManager : MonoBehaviour
{
    private PlayerStats stats;
    private Health health;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        health = GetComponent<Health>();
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        switch (upgrade.upgradeType)
        {
            case UpgradeType.MultiFireSide:
                stats.AddSideProjectiles(upgrade.intValue);
                break;

            case UpgradeType.MultiFireBack:
                stats.AddBackProjectiles(upgrade.intValue);
                break;

            case UpgradeType.ExtraLife:
                stats.AddExtraLife(upgrade.intValue);
                break;

            case UpgradeType.PierceEnemies:
                stats.AddPierce(upgrade.intValue);
                break;

            case UpgradeType.OneShotChance:
                stats.AddOneShotChance(upgrade.value);
                break;

            case UpgradeType.CritChance:
                stats.AddCritChance(upgrade.value);
                break;

            case UpgradeType.HeadshotChance:
                stats.AddHeadshotChance(upgrade.value);
                break;

            case UpgradeType.AttackSpeed:
                stats.AddAttackSpeed(upgrade.value);
                break;

            case UpgradeType.Damage:
                stats.AddDamage(upgrade.value);
                break;

            case UpgradeType.LowHealthDamage:
                stats.AddLowHealthDamage(upgrade.value);
                break;

            case UpgradeType.LowHealthAttackSpeed:
                stats.AddLowHealthAttackSpeed(upgrade.value);
                break;

            case UpgradeType.Rage:
                stats.ActivateRage(upgrade.duration, 1.6f, 1.6f, 1.25f);
                break;

            case UpgradeType.HealNow:
                health.Heal(upgrade.value);
                break;

            case UpgradeType.MaxHealth:
                health.SetMaxHealth(health.MaxHealth + upgrade.value, false);
                health.Heal(upgrade.value);
                break;

            case UpgradeType.BounceEnemies:
                stats.AddEnemyBounce(upgrade.intValue);
                break;

            case UpgradeType.BounceWalls:
                stats.AddWallBounce(upgrade.intValue);
                break;

            case UpgradeType.HealOnKill:
                stats.AddHealOnKill(upgrade.value);
                break;
        }
    }
}
