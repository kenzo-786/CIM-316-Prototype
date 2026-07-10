using System.Collections.Generic;
using UnityEngine;

public class UpgradeChoiceGenerator : MonoBehaviour
{
    [SerializeField] private UpgradeData[] upgradePool;
    [SerializeField] private UpgradeRarityWeights rarityWeights;
    [SerializeField] private UpgradeTarget currentCharacterTarget = UpgradeTarget.Any;

    public List<UpgradeData> GenerateChoices(int choiceCount, PlayerUpgradeInventory inventory, bool starterRoom)
    {
        List<UpgradeData> candidates = BuildCandidateList(inventory, starterRoom);
        List<UpgradeData> choices = new List<UpgradeData>();

        while (choices.Count < choiceCount && candidates.Count > 0)
        {
            UpgradeData picked = PickWeighted(candidates);
            choices.Add(picked);
            candidates.Remove(picked);
        }

        return choices;
    }

    public void SetCharacterTarget(UpgradeTarget target)
    {
        currentCharacterTarget = target;
    }

    private List<UpgradeData> BuildCandidateList(PlayerUpgradeInventory inventory, bool starterRoom)
    {
        List<UpgradeData> candidates = new List<UpgradeData>();

        foreach (UpgradeData upgrade in upgradePool)
        {
            if (upgrade == null)
                continue;

            if (starterRoom && !upgrade.canAppearInStarterRoom)
                continue;

            if (!starterRoom && !upgrade.canAppearOnLevelUp)
                continue;

            if (upgrade.target != UpgradeTarget.Any &&
                currentCharacterTarget != UpgradeTarget.Any &&
                upgrade.target != currentCharacterTarget)
                continue;

            if (inventory != null && !inventory.CanTake(upgrade))
                continue;

            candidates.Add(upgrade);
        }

        return candidates;
    }

    private UpgradeData PickWeighted(List<UpgradeData> candidates)
    {
        int totalWeight = 0;

        foreach (UpgradeData candidate in candidates)
            totalWeight += rarityWeights != null ? rarityWeights.GetWeight(candidate.rarity) : 1;

        int roll = Random.Range(0, totalWeight);
        int running = 0;

        foreach (UpgradeData candidate in candidates)
        {
            running += rarityWeights != null ? rarityWeights.GetWeight(candidate.rarity) : 1;

            if (roll < running)
                return candidate;
        }

        return candidates[candidates.Count - 1];
    }
}
