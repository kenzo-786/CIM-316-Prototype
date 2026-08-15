using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    [Header("Upgrade Catalogue")]
    [SerializeField] private MetaUpgradeData[] upgrades;

    [Header("Study Credit Rewards")]
    [SerializeField, Min(0)] private int creditsPerClearedRoom = 2;
    [SerializeField, Min(0)] private int eliteRoomBonus = 3;
    [SerializeField, Min(0)] private int bossRoomBonus = 10;
    [SerializeField, Min(0)] private int finalRoomBonus = 15;
    [SerializeField, Min(0)] private int runVictoryBonus = 20;

    public static MetaProgressionManager Instance { get; private set; }

    public event Action<int> OnCreditsChanged;
    public event Action<MetaUpgradeData> OnUpgradePurchased;

    public int StudyCredits { get; private set; }
    public IReadOnlyList<MetaUpgradeData> Upgrades => upgrades;

    private const string CreditsKey = "MetaProgression.StudyCredits";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        if (!IsDedicatedHost())
        {
            CreateDedicatedHost();
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        StudyCredits = PlayerPrefs.GetInt(CreditsKey, 0);
    }

    private bool IsDedicatedHost()
    {
        return transform.parent == null &&
               transform.childCount == 0 &&
               GetComponents<Component>().Length == 2;
    }

    private void CreateDedicatedHost()
    {
        GameObject host = new GameObject("MetaProgressionManager");
        MetaProgressionManager persistentManager =
            host.AddComponent<MetaProgressionManager>();

        persistentManager.upgrades = upgrades;
        persistentManager.creditsPerClearedRoom = creditsPerClearedRoom;
        persistentManager.eliteRoomBonus = eliteRoomBonus;
        persistentManager.bossRoomBonus = bossRoomBonus;
        persistentManager.finalRoomBonus = finalRoomBonus;
        persistentManager.runVictoryBonus = runVictoryBonus;

        Destroy(this);
    }

    public int GetLevel(MetaUpgradeData upgrade)
    {
        if (upgrade == null)
            return 0;

        return PlayerPrefs.GetInt(GetLevelKey(upgrade), 0);
    }

    public bool IsMaxed(MetaUpgradeData upgrade)
    {
        return upgrade == null || GetLevel(upgrade) >= upgrade.maxLevel;
    }

    public int GetCost(MetaUpgradeData upgrade)
    {
        if (upgrade == null || IsMaxed(upgrade))
            return -1;

        return upgrade.baseCost + GetLevel(upgrade) * upgrade.costIncreasePerLevel;
    }

    public bool CanPurchase(MetaUpgradeData upgrade)
    {
        int cost = GetCost(upgrade);
        return cost >= 0 && StudyCredits >= cost;
    }

    public bool TryPurchase(MetaUpgradeData upgrade)
    {
        if (!CanPurchase(upgrade))
            return false;

        int cost = GetCost(upgrade);

        StudyCredits -= cost;

        int nextLevel = GetLevel(upgrade) + 1;
        PlayerPrefs.SetInt(GetLevelKey(upgrade), nextLevel);
        Save();

        OnCreditsChanged?.Invoke(StudyCredits);
        OnUpgradePurchased?.Invoke(upgrade);

        return true;
    }

    public int GrantRunRewards(
        int clearedRooms,
        int eliteRooms,
        int bossRooms,
        int finalRooms,
        bool won
    )
    {
        int reward =
            Mathf.Max(0, clearedRooms) * creditsPerClearedRoom +
            Mathf.Max(0, eliteRooms) * eliteRoomBonus +
            Mathf.Max(0, bossRooms) * bossRoomBonus +
            Mathf.Max(0, finalRooms) * finalRoomBonus;

        if (won)
            reward += runVictoryBonus;

        if (reward <= 0)
            return 0;

        StudyCredits += reward;
        Save();

        OnCreditsChanged?.Invoke(StudyCredits);

        return reward;
    }

    public void ApplyToPlayer(GameObject player)
    {
        if (player == null)
            return;

        Health health = player.GetComponent<Health>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerStats stats = player.GetComponent<PlayerStats>();

        float healthBonus = GetTotalValue(MetaUpgradeType.MaxHealth);
        float moveSpeedBonus = GetTotalValue(MetaUpgradeType.MoveSpeed);
        float damageBonus = GetTotalValue(MetaUpgradeType.DamageMultiplier);
        int extraLives = Mathf.RoundToInt(GetTotalValue(MetaUpgradeType.ExtraLife));

        if (health != null && healthBonus > 0f)
            health.SetMaxHealth(health.MaxHealth + healthBonus, true);

        if (movement != null && moveSpeedBonus > 0f)
            movement.SetMoveSpeed(movement.MoveSpeed + moveSpeedBonus);

        if (stats != null && extraLives > 0)
            stats.AddExtraLife(extraLives);

        float damageMultiplier = Mathf.Max(0.1f, 1f + damageBonus);

        PlayerWeaponBase[] weapons =
            player.GetComponentsInChildren<PlayerWeaponBase>(true);

        foreach (PlayerWeaponBase weapon in weapons)
            weapon.SetPermanentDamageMultiplier(damageMultiplier);
    }

    public void ResetProgression()
    {
        StudyCredits = 0;
        PlayerPrefs.DeleteKey(CreditsKey);

        if (upgrades != null)
        {
            foreach (MetaUpgradeData upgrade in upgrades)
            {
                if (upgrade != null)
                    PlayerPrefs.DeleteKey(GetLevelKey(upgrade));
            }
        }

        PlayerPrefs.Save();
        OnCreditsChanged?.Invoke(StudyCredits);
    }

    private float GetTotalValue(MetaUpgradeType type)
    {
        if (upgrades == null)
            return 0f;

        float total = 0f;

        foreach (MetaUpgradeData upgrade in upgrades)
        {
            if (upgrade == null || upgrade.upgradeType != type)
                continue;

            total += GetLevel(upgrade) * upgrade.valuePerLevel;
        }

        return total;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(CreditsKey, StudyCredits);
        PlayerPrefs.Save();
    }

    private string GetLevelKey(MetaUpgradeData upgrade)
    {
        return "MetaProgression.Level." + upgrade.Id;
    }
}
