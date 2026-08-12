using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LevelUpUpgradeManager : MonoBehaviour
{
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private PlayerUpgradeManager upgradeManager;
    [SerializeField] private PlayerUpgradeInventory upgradeInventory;
    [SerializeField] private UpgradeChoiceGenerator choiceGenerator;
    [SerializeField] private UpgradeSelectionUI selectionUI;
    [SerializeField] private int choicesPerLevel = 3;
    [SerializeField] private bool pauseGameDuringChoice = true;

    private readonly Queue<int> pendingLevelUps = new Queue<int>();
    private bool showingChoice;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (playerExperience == null)
            playerExperience = FindObjectOfType<PlayerExperience>();

        if (upgradeManager == null)
            upgradeManager = FindObjectOfType<PlayerUpgradeManager>();

        if (upgradeInventory == null)
            upgradeInventory = FindObjectOfType<PlayerUpgradeInventory>();

        if (choiceGenerator == null)
            choiceGenerator = FindObjectOfType<UpgradeChoiceGenerator>();

        if (selectionUI == null)
            selectionUI = FindObjectOfType<UpgradeSelectionUI>();
    }

    private void OnEnable()
    {
        if (playerExperience != null)
            playerExperience.OnLevelUp += QueueLevelUpChoice;
    }

    private void OnDisable()
    {
        if (playerExperience != null)
            playerExperience.OnLevelUp -= QueueLevelUpChoice;
    }

    public void QueueLevelUpChoice(int level)
    {
        pendingLevelUps.Enqueue(level);
        TryShowNextChoice();
    }

    public void QueueLevelUpChoice()
    {
        pendingLevelUps.Enqueue(0);
        TryShowNextChoice();
    }

    private void TryShowNextChoice()
    {
        if (showingChoice || pendingLevelUps.Count == 0)
            return;

        pendingLevelUps.Dequeue();
        showingChoice = true;

        if (pauseGameDuringChoice)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        List<UpgradeData> choices = choiceGenerator != null
            ? choiceGenerator.GenerateChoices(choicesPerLevel, upgradeInventory, false)
            : new List<UpgradeData>();

        if (choices.Count == 0)
        {
            FinishCurrentChoice();
            return;
        }

        if (selectionUI != null)
            selectionUI.ShowChoices(choices, upgradeInventory, SelectUpgrade);
    }

    private void SelectUpgrade(UpgradeData upgrade)
    {
        bool accepted = upgradeInventory == null || upgradeInventory.TryAddUpgrade(upgrade, out _);

        if (accepted && upgradeManager != null && upgrade != null)
            upgradeManager.ApplyUpgrade(upgrade);

        if (selectionUI != null)
            selectionUI.Hide();

        FinishCurrentChoice();
    }

    private void FinishCurrentChoice()
    {
        if (pauseGameDuringChoice)
            Time.timeScale = previousTimeScale;

        showingChoice = false;
        TryShowNextChoice();
    }
}
