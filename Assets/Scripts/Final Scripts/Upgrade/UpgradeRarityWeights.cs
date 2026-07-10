using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Upgrades/Rarity Weights")]
public class UpgradeRarityWeights : ScriptableObject
{
    [SerializeField] private int commonWeight = 70;
    [SerializeField] private int rareWeight = 20;
    [SerializeField] private int epicWeight = 8;
    [SerializeField] private int legendaryWeight = 2;

    public int GetWeight(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Rare:
                return rareWeight;

            case UpgradeRarity.Epic:
                return epicWeight;

            case UpgradeRarity.Legendary:
                return legendaryWeight;

            default:
                return commonWeight;
        }
    }
}
