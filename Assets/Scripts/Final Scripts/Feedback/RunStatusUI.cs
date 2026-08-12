using System.Text;
using TMPro;
using UnityEngine;

public class RunStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text upgradesText;
    [SerializeField] private Health health;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerUpgradeInventory inventory;

    private void Awake()
    {
        FindReferences();
    }

    public void Refresh()
    {
        FindReferences();
        UpdateStats();
        UpdateUpgrades();
    }

    private void FindReferences()
    {
        if (movement == null)
            movement = FindObjectOfType<PlayerMovement>();

        if (movement != null)
        {
            if (health == null)
                health = movement.GetComponent<Health>();

            if (stats == null)
                stats = movement.GetComponent<PlayerStats>();

            if (inventory == null)
                inventory = movement.GetComponent<PlayerUpgradeInventory>();
        }
    }

    private void UpdateStats()
    {
        if (statsText == null)
            return;

        if (health == null || movement == null || stats == null)
        {
            statsText.text = "Player data unavailable.";
            return;
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("<b>CURRENT STATS</b>");
        builder.AppendLine();
        builder.AppendLine("Health: " + Mathf.CeilToInt(health.CurrentHealth) + " / " + Mathf.CeilToInt(health.MaxHealth));
        builder.AppendLine("Move Speed: " + movement.EffectiveMoveSpeed.ToString("0.0"));
        builder.AppendLine("Damage: x" + stats.GetDamageMultiplier().ToString("0.00") + "   Attack: x" + stats.GetAttackSpeedMultiplier().ToString("0.00"));
        builder.AppendLine("Crit: " + (stats.CritChance * 100f).ToString("0") + "%   Headshot: " + (stats.HeadshotChance * 100f).ToString("0") + "%");
        builder.AppendLine("One-Shot Chance: " + (stats.OneShotChance * 100f).ToString("0") + "%");
        builder.AppendLine();
        builder.AppendLine("<b>PROJECTILES</b>");
        builder.AppendLine();
        builder.AppendLine("Side: " + stats.SideProjectiles + "   Back: " + stats.BackProjectiles + "   Pierce: " + stats.PierceCount);
        builder.AppendLine("Enemy Bounce: " + stats.EnemyBounceCount + "   Wall Bounce: " + stats.WallBounceCount);
        builder.AppendLine();
        builder.AppendLine("<b>SURVIVAL</b>");
        builder.AppendLine();
        builder.AppendLine("Extra Lives: " + stats.ExtraLives + "   Heal On Kill: " + (stats.HealOnKillPercent * 100f).ToString("0") + "%");

        statsText.text = builder.ToString();
    }

    private void UpdateUpgrades()
    {
        if (upgradesText == null)
            return;

        if (inventory == null || inventory.TakenUpgrades.Count == 0)
        {
            upgradesText.text = "<b>UPGRADES TAKEN</b>\n\nNo upgrades selected yet.";
            return;
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("<b>UPGRADES TAKEN</b>");
        builder.AppendLine();

        foreach (UpgradeData upgrade in inventory.TakenUpgrades)
        {
            if (upgrade == null)
                continue;

            int level = inventory.GetLevel(upgrade);

            builder.AppendLine(
                upgrade.upgradeName +
                "  Lv " +
                level +
                (upgrade.maxLevel > 0 ? "/" + upgrade.maxLevel : "")
            );
        }

        upgradesText.text = builder.ToString();
    }
}