using System.Collections.Generic;
using UnityEngine;

public class LevelUpUpgradeManager : MonoBehaviour
{
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private PlayerUpgradeManager playerUpgradeManager;
    [SerializeField] private UpgradeSelectionUI upgradeSelectionUI;
    [SerializeField] private UpgradeData[] upgradePool;
    [SerializeField] private int choiceCount = 3;

    private void OnEnable()
    {
        playerExperience.OnLevelUp += ShowLevelUpChoices;
    }

    private void OnDisable()
    {
        playerExperience.OnLevelUp -= ShowLevelUpChoices;
    }

    private void ShowLevelUpChoices(int newLevel)
    {
        List<UpgradeData> choices = GetRandomChoices();

        Time.timeScale = 0f;

        upgradeSelectionUI.Show(choices, upgrade =>
        {
            playerUpgradeManager.ApplyUpgrade(upgrade);
            upgradeSelectionUI.Hide();
            Time.timeScale = 1f;
        });
    }

    private List<UpgradeData> GetRandomChoices()
    {
        List<UpgradeData> available = new List<UpgradeData>(upgradePool);
        List<UpgradeData> selected = new List<UpgradeData>();

        int amount = Mathf.Min(choiceCount, available.Count);

        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            selected.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }

        return selected;
    }
}
