using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthHud : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private string fallbackBossName = "Boss";

    private BossHealthTarget currentTarget;

    private void Awake()
    {
        Hide();
    }

    public void Show(BossHealthTarget target, string bossName = "")
    {
        ClearTarget();
        currentTarget = target;

        if (currentTarget == null)
        {
            Hide();
            return;
        }

        currentTarget.OnBossHealthChanged += UpdateHealth;
        currentTarget.OnBossDefeated += Hide;

        if (root != null)
            root.SetActive(true);

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(bossName) ? fallbackBossName : bossName;

        UpdateHealth(currentTarget.CurrentHealth, currentTarget.MaxHealth);
    }

    public void Hide()
    {
        ClearTarget();

        if (root != null)
            root.SetActive(false);
    }

    private void UpdateHealth(float current, float max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max <= 0f ? 0f : Mathf.Clamp01(current / max);
    }

    private void ClearTarget()
    {
        if (currentTarget == null)
            return;

        currentTarget.OnBossHealthChanged -= UpdateHealth;
        currentTarget.OnBossDefeated -= Hide;
        currentTarget = null;
    }
}
