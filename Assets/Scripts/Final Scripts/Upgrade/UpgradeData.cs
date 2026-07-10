using UnityEngine;

public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum UpgradeType
{
    MultiFireSide,
    MultiFireBack,
    ExtraLife,
    PierceEnemies,
    OneShotChance,
    CritChance,
    HeadshotChance,
    AttackSpeed,
    Damage,
    LowHealthDamage,
    LowHealthAttackSpeed,
    Rage,
    HealNow,
    MaxHealth,
    BounceEnemies,
    BounceWalls,
    HealOnKill
}

public enum UpgradeTarget
{
    Any,
    RulerOnly,
    EraserOnly
}

[CreateAssetMenu(menuName = "Deadline Dungeon/Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Rules")]
    public UpgradeRarity rarity = UpgradeRarity.Common;
    public UpgradeType upgradeType;
    public UpgradeTarget target = UpgradeTarget.Any;

    [Tooltip("0 or less means this upgrade can appear forever.")]
    public int maxLevel = 5;

    [Header("Values")]
    public float value = 0.1f;
    public int intValue = 1;
    public float duration = 5f;

    public string Id
    {
        get
        {
            if (!string.IsNullOrEmpty(upgradeId))
                return upgradeId;

            return name;
        }
    }
}
