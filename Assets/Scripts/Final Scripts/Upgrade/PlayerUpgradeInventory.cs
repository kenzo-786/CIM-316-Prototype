using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeInventory : MonoBehaviour
{
    private readonly Dictionary<string, int> levelsByUpgradeId = new Dictionary<string, int>();

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

    public int AddUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
            return 0;

        int nextLevel = GetLevel(upgrade) + 1;
        levelsByUpgradeId[upgrade.Id] = nextLevel;
        return nextLevel;
    }

    public void Clear()
    {
        levelsByUpgradeId.Clear();
    }
}
