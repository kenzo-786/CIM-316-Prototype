using System.Collections.Generic;
using System;
using UnityEngine;

public class UpgradeSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private UpgradeChoiceButton[] choiceButtons;

    private Action<UpgradeData> onSelected;

    public void Show(List<UpgradeData> upgrades, Action<UpgradeData> selectedCallback)
    {
        onSelected = selectedCallback;
        root.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < upgrades.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].Setup(upgrades[i], SelectUpgrade);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    private void SelectUpgrade(UpgradeData upgrade)
    {
        onSelected?.Invoke(upgrade);
    }
}
