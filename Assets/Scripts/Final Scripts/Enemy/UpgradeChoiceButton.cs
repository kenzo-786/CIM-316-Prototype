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
        if (button == null)
            button = GetComponent<Button>();

        if (upgradeData == null)
        {
            Debug.LogError("UpgradeChoiceButton received null UpgradeData.", this);
            return;
        }

        upgrade = upgradeData;
        onClicked = clickedCallback;

        if (nameText != null)
            nameText.text = upgrade.upgradeName;
        else
            Debug.LogError("UpgradeChoiceButton missing Name Text.", this);

        if (descriptionText != null)
            descriptionText.text = upgrade.description;
        else
            Debug.LogError("UpgradeChoiceButton missing Description Text.", this);

        if (rarityText != null)
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
