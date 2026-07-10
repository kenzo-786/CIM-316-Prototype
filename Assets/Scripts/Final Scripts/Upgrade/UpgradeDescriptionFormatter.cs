using UnityEngine;

public static class UpgradeDescriptionFormatter 
{
    public static string Format(UpgradeData upgrade, int currentLevel)
    {
        if (upgrade == null)
            return string.Empty;

        string text = upgrade.description;
        int nextLevel = currentLevel + 1;

        text = text.Replace("{value}", upgrade.value.ToString("0.##"));
        text = text.Replace("{int}", upgrade.intValue.ToString());
        text = text.Replace("{duration}", upgrade.duration.ToString("0.##"));
        text = text.Replace("{level}", nextLevel.ToString());
        text = text.Replace("{max}", upgrade.maxLevel.ToString());

        return text;
    }
}
