using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UpgradeChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image iconImage;

    private Button button;
    private UpgradeData upgrade;
    private Action<UpgradeData> onClicked;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(UpgradeData upgradeData, Action<UpgradeData> clickedCallback)
    {
        upgrade = upgradeData;
        onClicked = clickedCallback;

        nameText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        rarityText.text = upgrade.rarity.ToString();

        if (iconImage != null)
        {
            iconImage.sprite = upgrade.icon;
            iconImage.enabled = upgrade.icon != null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked?.Invoke(upgrade));
    }
}
