using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaProgressionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private MetaUpgradeButton[] upgradeButtons;

    private MetaProgressionManager manager;
    private bool runtimeLayoutBuilt;

    private void OnEnable()
    {
        EnsureRuntimeLayout();
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

        if (upgradeButtons == null)
            return;

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

    private void EnsureRuntimeLayout()
    {
        if (runtimeLayoutBuilt || (upgradeButtons != null && upgradeButtons.Length > 0))
            return;

        runtimeLayoutBuilt = true;

        RectTransform panel = transform as RectTransform;

        if (panel == null || TMP_Settings.defaultFontAsset == null)
            return;

        GameObject contentObject = new GameObject(
            "Meta Progression Content",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup)
        );

        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.SetParent(panel, false);
        content.anchorMin = new Vector2(0.5f, 0.5f);
        content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.anchoredPosition = new Vector2(30f, -20f);
        content.sizeDelta = new Vector2(1120f, 690f);

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 28, 28);
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text title = CreateText(content, "STUDY UPGRADES", 40, TextAlignmentOptions.Center, new Color(0.94f, 0.9f, 0.62f));
        SetHeight(title.rectTransform, 58f);

        creditsText = CreateText(content, "STUDY CREDITS: 0", 25, TextAlignmentOptions.Center, Color.white);
        SetHeight(creditsText.rectTransform, 38f);

        messageText = CreateText(content, string.Empty, 18, TextAlignmentOptions.Center, new Color(0.85f, 0.92f, 0.9f));
        SetHeight(messageText.rectTransform, 28f);

        int count = MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.Upgrades != null
            ? MetaProgressionManager.Instance.Upgrades.Count
            : 4;

        upgradeButtons = new MetaUpgradeButton[count];

        for (int i = 0; i < count; i++)
            upgradeButtons[i] = CreateRuntimeUpgradeButton(content);
    }

    private MetaUpgradeButton CreateRuntimeUpgradeButton(RectTransform parent)
    {
        GameObject cardObject = new GameObject(
            "Meta Upgrade Card",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement),
            typeof(MetaUpgradeButton)
        );

        RectTransform card = cardObject.GetComponent<RectTransform>();
        card.SetParent(parent, false);

        Image background = cardObject.GetComponent<Image>();
        background.color = new Color(0.055f, 0.16f, 0.15f, 0.94f);

        LayoutElement layoutElement = cardObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 112f;

        Button button = cardObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.88f, 1f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.9f, 0.72f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;

        TMP_Text name = CreateText(card, string.Empty, 25, TextAlignmentOptions.Left, new Color(0.96f, 0.91f, 0.56f));
        SetAnchors(name.rectTransform, new Vector2(0f, 0.56f), new Vector2(0.72f, 1f), new Vector2(22f, -12f), new Vector2(-8f, -6f));

        TMP_Text description = CreateText(card, string.Empty, 18, TextAlignmentOptions.Left, Color.white);
        description.enableWordWrapping = true;
        SetAnchors(description.rectTransform, new Vector2(0f, 0f), new Vector2(0.72f, 0.6f), new Vector2(22f, 12f), new Vector2(-8f, -4f));

        TMP_Text level = CreateText(card, string.Empty, 19, TextAlignmentOptions.Right, new Color(0.78f, 0.94f, 0.9f));
        SetAnchors(level.rectTransform, new Vector2(0.7f, 0.52f), new Vector2(1f, 1f), new Vector2(4f, -12f), new Vector2(-22f, -6f));

        TMP_Text cost = CreateText(card, string.Empty, 18, TextAlignmentOptions.Right, Color.white);
        SetAnchors(cost.rectTransform, new Vector2(0.7f, 0f), new Vector2(1f, 0.5f), new Vector2(4f, 12f), new Vector2(-22f, -6f));

        MetaUpgradeButton upgradeButton = cardObject.GetComponent<MetaUpgradeButton>();
        upgradeButton.ConfigureRuntime(button, name, description, level, cost);
        return upgradeButton;
    }

    private TMP_Text CreateText(Transform parent, string value, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private void SetHeight(RectTransform rect, float height)
    {
        LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = height;
    }

    private void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
