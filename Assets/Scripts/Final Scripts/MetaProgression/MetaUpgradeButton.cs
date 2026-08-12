using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MetaUpgradeButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;

    private MetaUpgradeData upgrade;
    private MetaProgressionUI owner;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(Purchase);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Purchase);
    }

    public void Setup(MetaUpgradeData data, MetaProgressionUI ui)
    {
        upgrade = data;
        owner = ui;

        if (upgrade == null || MetaProgressionManager.Instance == null)
        {
            Clear();
            return;
        }

        MetaProgressionManager manager = MetaProgressionManager.Instance;
        int level = manager.GetLevel(upgrade);
        bool maxed = manager.IsMaxed(upgrade);
        int cost = manager.GetCost(upgrade);

        if (button != null)
            button.interactable = !maxed && manager.CanPurchase(upgrade);

        if (nameText != null)
            nameText.text = upgrade.upgradeName;

        if (descriptionText != null)
            descriptionText.text = upgrade.description;

        if (levelText != null)
            levelText.text = "Level " + level + "/" + upgrade.maxLevel;

        if (costText != null)
            costText.text = maxed ? "MAXED" : cost + " CREDITS";

        if (iconImage != null)
        {
            iconImage.sprite = upgrade.icon;
            iconImage.enabled = upgrade.icon != null;
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        upgrade = null;
        owner = null;

        if (button != null)
            button.interactable = false;

        if (nameText != null)
            nameText.text = string.Empty;

        if (descriptionText != null)
            descriptionText.text = string.Empty;

        if (levelText != null)
            levelText.text = string.Empty;

        if (costText != null)
            costText.text = string.Empty;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        gameObject.SetActive(false);
    }

    private void Purchase()
    {
        if (upgrade == null || MetaProgressionManager.Instance == null)
            return;

        bool purchased = MetaProgressionManager.Instance.TryPurchase(upgrade);
        owner?.HandlePurchaseResult(purchased, upgrade);
    }
}
