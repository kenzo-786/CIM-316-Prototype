using System.Collections.Generic;
using System;
using UnityEngine;

public class UpgradeSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private UpgradeChoiceButton[] choiceButtons;

    private Action<UpgradeData> selectedCallback;
    private PlayerUpgradeInventory inventory;

    private void Awake()
    {
        Hide();
    }

    public void ShowChoices(List<UpgradeData> choices, PlayerUpgradeInventory playerInventory, Action<UpgradeData> onSelected)
    {
        inventory = playerInventory;
        selectedCallback = onSelected;

        if (root != null)
            root.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            UpgradeData upgrade = choices != null && i < choices.Count ? choices[i] : null;
            int currentLevel = inventory != null && upgrade != null ? inventory.GetLevel(upgrade) : 0;

            if (choiceButtons[i] != null)
                choiceButtons[i].Setup(upgrade, currentLevel, SelectUpgrade);
        }
    }

    public void Show(List<UpgradeData> choices, PlayerUpgradeInventory playerInventory, Action<UpgradeData> onSelected)
    {
        ShowChoices(choices, playerInventory, onSelected);
    }

    public void Show(List<UpgradeData> choices, Action<UpgradeData> onSelected)
    {
        ShowChoices(choices, inventory, onSelected);
    }

    public void Show(UpgradeData[] choices, Action<UpgradeData> onSelected)
    {
        List<UpgradeData> choiceList = choices != null ? new List<UpgradeData>(choices) : new List<UpgradeData>();
        ShowChoices(choiceList, inventory, onSelected);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (choiceButtons != null)
        {
            foreach (UpgradeChoiceButton button in choiceButtons)
            {
                if (button != null)
                    button.Clear();
            }
        }

        selectedCallback = null;
    }

    private void SelectUpgrade(UpgradeData upgrade)
    {
        selectedCallback?.Invoke(upgrade);
    }
}
