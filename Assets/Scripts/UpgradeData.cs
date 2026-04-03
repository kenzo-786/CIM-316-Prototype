using UnityEngine;

public enum UpgradeType { MoveSpeed, FireRate, Damage, PickupRange, MaxHealth, BulletSpeed, HealthRegen, BulletSize, BulletLife, Armor }
public enum Rarity { Common, Rare, Legendary }

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "DeadlineDungeon/Upgrade")]

public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public UpgradeType type;
    public float value;
    public Rarity rarity;

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Rare: return new Color(0.2f, 0.6f, 1f);
            case Rarity.Legendary: return new Color(1f, 0.8f, 0f); 
            default: return Color.white;
        }
    }

    public int GetWeight()
    {
        switch (rarity)
        {
            case Rarity.Common: return 70;
            case Rarity.Rare: return 25;
            case Rarity.Legendary: return 5;
            default: return 0;
        }
    }
}
