using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private bool playGoesToCharacterSelect = true;

    private void Awake()
    {
        ShowMain();
    }

    public void Play()
    {
        string sceneName = playGoesToCharacterSelect ? characterSelectSceneName : gameSceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void ShowMain()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
