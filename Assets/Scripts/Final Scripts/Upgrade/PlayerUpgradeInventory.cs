using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeInventory : MonoBehaviour
{
    private readonly Dictionary<string, int> levelsByUpgradeId = new Dictionary<string, int>();
    private readonly Dictionary<string, UpgradeData> upgradesById = new Dictionary<string, UpgradeData>();
    private readonly List<UpgradeData> takenUpgrades = new List<UpgradeData>();

    public IReadOnlyList<UpgradeData> TakenUpgrades => takenUpgrades;

    public int GetLevel(UpgradeData upgrade)
    {
        if (upgrade == null)
            return 0;

        return levelsByUpgradeId.TryGetValue(upgrade.Id, out int level) ? level : 0;
    }

    public bool CanTake(UpgradeData upgrade)
    {
        if (upgrade == null)
            return false;

        if (upgrade.maxLevel <= 0)
            return true;

        return GetLevel(upgrade) < upgrade.maxLevel;
    }

    public bool TryAddUpgrade(UpgradeData upgrade, out int newLevel)
    {
        newLevel = GetLevel(upgrade);

        if (!CanTake(upgrade))
            return false;

        newLevel++;
        levelsByUpgradeId[upgrade.Id] = newLevel;

        if (!upgradesById.ContainsKey(upgrade.Id))
        {
            upgradesById.Add(upgrade.Id, upgrade);
            takenUpgrades.Add(upgrade);
        }

        return true;
    }

    public int AddUpgrade(UpgradeData upgrade)
    {
        return TryAddUpgrade(upgrade, out int newLevel)
            ? newLevel
            : GetLevel(upgrade);
    }

    public void Clear()
    {
        levelsByUpgradeId.Clear();
        upgradesById.Clear();
        takenUpgrades.Clear();
    }
}
