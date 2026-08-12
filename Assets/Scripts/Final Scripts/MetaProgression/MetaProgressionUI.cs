using TMPro;
using UnityEngine;
public class MetaProgressionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private MetaUpgradeButton[] upgradeButtons;

    private MetaProgressionManager manager;

    private void OnEnable()
    {
        manager = MetaProgressionManager.Instance;

        if (manager != null)
        {
            manager.OnCreditsChanged += HandleCreditsChanged;
            manager.OnUpgradePurchased += HandleUpgradePurchased;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnCreditsChanged -= HandleCreditsChanged;
            manager.OnUpgradePurchased -= HandleUpgradePurchased;
        }
    }

    public void Refresh()
    {
        manager = MetaProgressionManager.Instance;

        if (manager == null)
            return;

        if (creditsText != null)
            creditsText.text = "STUDY CREDITS: " + manager.StudyCredits;

        if (messageText != null)
            messageText.text = string.Empty;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            MetaUpgradeData upgrade =
                manager.Upgrades != null && i < manager.Upgrades.Count
                    ? manager.Upgrades[i]
                    : null;

            if (upgradeButtons[i] == null)
                continue;

            if (upgrade != null)
                upgradeButtons[i].Setup(upgrade, this);
            else
                upgradeButtons[i].Clear();
        }
    }

    public void HandlePurchaseResult(bool purchased, MetaUpgradeData upgrade)
    {
        Refresh();

        if (messageText != null)
        {
            messageText.text = purchased
                ? upgrade.upgradeName + " upgraded."
                : "Not enough Study Credits.";
        }
    }

    private void HandleCreditsChanged(int credits)
    {
        Refresh();
    }

    private void HandleUpgradePurchased(MetaUpgradeData upgrade)
    {
        Refresh();
    }
}
