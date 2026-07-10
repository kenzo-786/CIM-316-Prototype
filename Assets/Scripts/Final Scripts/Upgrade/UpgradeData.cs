using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId = "upgrade_id";
    public string upgradeName = "Upgrade";
    [TextArea] public string description = "Upgrade description.";
    public Sprite icon;

    [Header("Rules")]
    public UpgradeType upgradeType;
    public UpgradeRarity rarity = UpgradeRarity.Common;
    public UpgradeTarget target = UpgradeTarget.Any;
    public int maxLevel = 1;
    public bool canAppearInStarterRoom = true;
    public bool canAppearOnLevelUp = true;

    [Header("Values")]
    public float value = 1f;
    public int intValue = 1;
    public float duration = 0f;

    public string Id => string.IsNullOrWhiteSpace(upgradeId) ? name : upgradeId;
}
