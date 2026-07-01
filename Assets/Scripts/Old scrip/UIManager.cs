using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public PlayerController player;

    [Header("UI Elements")]
    public Image healthFill;
    public Image expFill;
    public TextMeshProUGUI levelText;

    [Header("Upgrade System")]
    public GameObject upgradePanel;
    public List<UpgradeData> allUpgrades;
    public UpgradeUIElement[] upgradeButtons;

    private List<UpgradeData> currentChoices = new List<UpgradeData>();

    void Start()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (healthFill != null)
        {
            healthFill.fillAmount = player.currentHealth / player.maxHealth;
        }

        if (expFill != null)
        {
            expFill.fillAmount = player.experience / player.expToLevelUp;
        }

        if (levelText != null)
        {
            levelText.text = "LEVEL: " + player.level;
        }
    }

    public void ShowUpgradeScreen()
    {
        if (upgradePanel == null || allUpgrades.Count < 3) return;

        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
        currentChoices.Clear();

        for (int i = 0; i < 3; i++)
        {
            UpgradeData selected = GetWeightedRandomUpgrade();
            int safety = 0;
            while (currentChoices.Contains(selected) && safety < 10)
            {
                selected = GetWeightedRandomUpgrade();
                safety++;
            }
            currentChoices.Add(selected);
            upgradeButtons[i].Setup(selected, this, i);
        }
    }

    UpgradeData GetWeightedRandomUpgrade()
    {
        int totalWeight = 0;
        foreach (var u in allUpgrades) totalWeight += u.GetWeight();

        int r = Random.Range(0, totalWeight);
        int currentSum = 0;

        foreach (var u in allUpgrades)
        {
            currentSum += u.GetWeight();
            if (r < currentSum) return u;
        }
        return allUpgrades[0];
    }

    public void SelectUpgrade(int index)
    {
        if (index >= currentChoices.Count) return;

        UpgradeData choice = currentChoices[index];
        ApplyUpgradeEffect(choice);

        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.MoveSpeed: player.moveSpeed += upgrade.value; break;
            case UpgradeType.FireRate: player.fireRate -= upgrade.value; break;
            case UpgradeType.Damage: player.bulletDamage += upgrade.value; break;
            case UpgradeType.PickupRange: player.collectionRange += upgrade.value; break;
            case UpgradeType.MaxHealth:
                player.maxHealth += upgrade.value;
                player.currentHealth += upgrade.value;
                break;
            case UpgradeType.BulletSpeed: player.bulletSpeed += upgrade.value; break;
            case UpgradeType.BulletSize: player.bulletSizeMultiplier += upgrade.value; break;
            case UpgradeType.HealthRegen: player.currentHealth = Mathf.Min(player.currentHealth + upgrade.value, player.maxHealth); break;
        }
        Debug.Log("Applied: " + upgrade.upgradeName);
    }
}

[System.Serializable]
public class UpgradeUIElement
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Image rarityPanel;
    public Button button;

    public void Setup(UpgradeData data, UIManager manager, int index)
    {
        titleText.text = data.upgradeName;
        titleText.color = data.GetRarityColor();
        descText.text = data.description;
        if (rarityPanel != null) rarityPanel.color = data.GetRarityColor();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => manager.SelectUpgrade(index));
    }
}
