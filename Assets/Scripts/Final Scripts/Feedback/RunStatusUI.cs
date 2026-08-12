using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RunStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text upgradesText;

    [SerializeField] private Health health;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerUpgradeInventory inventory;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    [Header("Button Pop")]
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    private Coroutine popCoroutine;

    private Dictionary<Button, Vector3> originalScales =
        new Dictionary<Button, Vector3>();

    private void Awake()
    {
        FindReferences();

        RegisterButton(backButton);
    }

    private void Update()
    {
        HandleBackButtonHover();

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            if (backButton != null)
                Back();
        }
    }

    public void Refresh()
    {
        FindReferences();

        UpdateStats();
        UpdateUpgrades();
    }

    public void Back()
    {
        PlayPop(backButton);

        PauseMenu pauseMenu =
            FindObjectOfType<PauseMenu>();

        if (pauseMenu != null)
            pauseMenu.ShowMenuContent();
    }

    private void HandleBackButtonHover()
    {
        if (EventSystem.current == null ||
            backButton == null)
            return;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button == backButton)
            {
                if (!IsPointerOverBackButton())
                    PlayPop(backButton);

                return;
            }
        }
    }

    private bool IsPointerOverBackButton()
    {
        RectTransform rect =
            backButton.GetComponent<RectTransform>();

        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            Input.mousePosition,
            null
        );
    }

    private void RegisterButton(Button button)
    {
        if (button == null)
            return;

        originalScales[button] =
            button.transform.localScale;
    }

    private void PlayPop(Button button)
    {
        if (button == null)
            return;

        if (!originalScales.ContainsKey(button))
            RegisterButton(button);

        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine =
            StartCoroutine(PopButton(button));
    }

    private IEnumerator PopButton(Button button)
    {
        Vector3 originalScale =
            originalScales[button];

        Vector3 enlargedScale =
            originalScale * popScale;

        float time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            button.transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    time / popSpeed
                );

            yield return null;
        }

        time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            button.transform.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    time / popSpeed
                );

            yield return null;
        }

        button.transform.localScale =
            originalScale;
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
                inventory =
                    movement.GetComponent<PlayerUpgradeInventory>();
        }
    }

    private void UpdateStats()
    {
        if (statsText == null)
            return;

        if (health == null ||
            movement == null ||
            stats == null)
        {
            statsText.text =
                "Player data unavailable.";

            return;
        }

        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine("<b>CURRENT STATS</b>");
        builder.AppendLine();

        builder.AppendLine(
            "Health: " +
            Mathf.CeilToInt(health.CurrentHealth) +
            " / " +
            Mathf.CeilToInt(health.MaxHealth)
        );

        builder.AppendLine(
            "Move Speed: " +
            movement.EffectiveMoveSpeed.ToString("0.0")
        );

        builder.AppendLine(
            "Damage: x" +
            stats.GetDamageMultiplier().ToString("0.00") +
            "   Attack: x" +
            stats.GetAttackSpeedMultiplier().ToString("0.00")
        );

        builder.AppendLine(
            "Crit: " +
            (stats.CritChance * 100f).ToString("0") +
            "%   Headshot: " +
            (stats.HeadshotChance * 100f).ToString("0") +
            "%"
        );

        builder.AppendLine(
            "One-Shot Chance: " +
            (stats.OneShotChance * 100f).ToString("0") +
            "%"
        );

        builder.AppendLine();

        builder.AppendLine("<b>PROJECTILES</b>");
        builder.AppendLine();

        builder.AppendLine(
            "Side: " +
            stats.SideProjectiles +
            "   Back: " +
            stats.BackProjectiles +
            "   Pierce: " +
            stats.PierceCount
        );

        builder.AppendLine(
            "Enemy Bounce: " +
            stats.EnemyBounceCount +
            "   Wall Bounce: " +
            stats.WallBounceCount
        );

        builder.AppendLine();

        builder.AppendLine("<b>SURVIVAL</b>");
        builder.AppendLine();

        builder.AppendLine(
            "Extra Lives: " +
            stats.ExtraLives +
            "   Heal On Kill: " +
            (stats.HealOnKillPercent * 100f).ToString("0") +
            "%"
        );

        statsText.text =
            builder.ToString();
    }

    private void UpdateUpgrades()
    {
        if (upgradesText == null)
            return;

        if (inventory == null ||
            inventory.TakenUpgrades.Count == 0)
        {
            upgradesText.text =
                "<b>UPGRADES TAKEN</b>\n\n" +
                "No upgrades selected yet.";

            return;
        }

        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine("<b>UPGRADES TAKEN</b>");
        builder.AppendLine();

        foreach (UpgradeData upgrade in
                 inventory.TakenUpgrades)
        {
            if (upgrade == null)
                continue;

            int level =
                inventory.GetLevel(upgrade);

            builder.AppendLine(
                upgrade.upgradeName +
                "  Lv " +
                level +
                (upgrade.maxLevel > 0
                    ? "/" + upgrade.maxLevel
                    : "")
            );
        }

        upgradesText.text =
            builder.ToString();
    }
}