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

[CreateAssetMenu(menuName = "Deadline Dungeon/Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    public UpgradeRarity rarity;
    public UpgradeType upgradeType;

    public float value = 0.1f;
    public int intValue = 1;
    public float duration = 5f;
}
