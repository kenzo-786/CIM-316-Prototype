using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerXpHud : MonoBehaviour
{
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;

    private void OnEnable()
    {
        if (playerExperience == null)
        {
            Debug.LogError("PlayerXpHud missing PlayerExperience.", this);
            return;
        }

        playerExperience.OnXpChanged += UpdateUI;
        UpdateUI(playerExperience.CurrentXp, playerExperience.XpToNextLevel, playerExperience.Level);
    }

    private void OnDisable()
    {
        if (playerExperience != null)
            playerExperience.OnXpChanged -= UpdateUI;
    }

    private void UpdateUI(int currentXp, int xpToNextLevel, int level)
    {
        float fill = xpToNextLevel <= 0 ? 0f : (float)currentXp / xpToNextLevel;

        if (xpFillImage != null)
            xpFillImage.fillAmount = Mathf.Clamp01(fill);

        if (levelText != null)
            levelText.text = "Level " + level;

        if (xpText != null)
            xpText.text = currentXp + " / " + xpToNextLevel;
    }
}
