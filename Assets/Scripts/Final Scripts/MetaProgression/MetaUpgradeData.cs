using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Meta/Meta Upgrade Data")]
public class MetaUpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId = "meta_upgrade_id";
    public string upgradeName = "Meta Upgrade";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Progression")]
    public MetaUpgradeType upgradeType;
    [Min(1)] public int maxLevel = 1;
    [Min(0)] public int baseCost = 10;
    [Min(0)] public int costIncreasePerLevel = 5;
    public float valuePerLevel = 1f;

    public string Id => string.IsNullOrWhiteSpace(upgradeId) ? name : upgradeId;
}
