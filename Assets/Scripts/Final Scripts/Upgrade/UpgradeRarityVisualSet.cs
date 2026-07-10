using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Upgrades/Rarity Visual Set")]
public class UpgradeRarityVisualSet : ScriptableObject
{
    [System.Serializable]
    public class RarityVisual
    {
        public UpgradeRarity rarity;
        public Color tint = Color.white;
        public Sprite cardBackground;
    }

    [SerializeField] private RarityVisual common = new RarityVisual { rarity = UpgradeRarity.Common };
    [SerializeField] private RarityVisual rare = new RarityVisual { rarity = UpgradeRarity.Rare };
    [SerializeField] private RarityVisual epic = new RarityVisual { rarity = UpgradeRarity.Epic };
    [SerializeField] private RarityVisual legendary = new RarityVisual { rarity = UpgradeRarity.Legendary };

    public Color GetTint(UpgradeRarity rarity)
    {
        return GetVisual(rarity).tint;
    }

    public Sprite GetCardBackground(UpgradeRarity rarity)
    {
        return GetVisual(rarity).cardBackground;
    }

    private RarityVisual GetVisual(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Rare:
                return rare;

            case UpgradeRarity.Epic:
                return epic;

            case UpgradeRarity.Legendary:
                return legendary;

            default:
                return common;
        }
    }
}
