using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UpgradeChoiceButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image rarityTintImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private UpgradeRarityVisualSet rarityVisuals;

    private UpgradeData upgrade;
    private Action<UpgradeData> clickedCallback;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(Click);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Click);
    }

    public void Setup(UpgradeData upgradeData, int currentLevel, Action<UpgradeData> onClicked)
    {
        upgrade = upgradeData;
        clickedCallback = onClicked;

        bool hasUpgrade = upgrade != null;

        if (button != null)
            button.interactable = hasUpgrade;

        if (!hasUpgrade)
        {
            Clear();
            return;
        }

        if (nameText != null)
            nameText.text = upgrade.upgradeName;

        if (descriptionText != null)
            descriptionText.text = UpgradeDescriptionFormatter.Format(upgrade, currentLevel);

        if (rarityText != null)
            rarityText.text = upgrade.rarity.ToString();

        if (levelText != null)
        {
            int nextLevel = currentLevel + 1;
            levelText.text = upgrade.maxLevel > 0
                ? "Lv " + nextLevel + "/" + upgrade.maxLevel
                : "Lv " + nextLevel;
        }

        if (iconImage != null)
        {
            iconImage.sprite = upgrade.icon;
            iconImage.enabled = upgrade.icon != null;
        }

        if (rarityVisuals != null)
        {
            if (cardBackgroundImage != null)
            {
                Sprite cardSprite = rarityVisuals.GetCardBackground(upgrade.rarity);
                cardBackgroundImage.sprite = cardSprite;
                cardBackgroundImage.enabled = cardSprite != null;
            }

            if (rarityTintImage != null)
                rarityTintImage.color = rarityVisuals.GetTint(upgrade.rarity);
        }
    }

    public void Clear()
    {
        upgrade = null;
        clickedCallback = null;

        if (nameText != null)
            nameText.text = string.Empty;

        if (descriptionText != null)
            descriptionText.text = string.Empty;

        if (rarityText != null)
            rarityText.text = string.Empty;

        if (levelText != null)
            levelText.text = string.Empty;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    private void Click()
    {
        if (upgrade != null)
            clickedCallback?.Invoke(upgrade);
    }
}
