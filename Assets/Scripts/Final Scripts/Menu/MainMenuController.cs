using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject progressionPanel;
    [SerializeField] private bool playGoesToCharacterSelect = true;

    private void Awake()
    {
        ShowMain();
    }

    public void Play()
    {
        string sceneName =
            playGoesToCharacterSelect
                ? characterSelectSceneName
                : gameSceneName;

        SceneManager.LoadScene(sceneName);
    }

    public void ShowMain()
    {
        SetPanelStates(true, false, false, false);
    }

    public void ShowSettings()
    {
        SetPanelStates(false, true, false, false);
    }

    public void ShowCredits()
    {
        SetPanelStates(false, false, true, false);
    }

    public void ShowProgression()
    {
        SetPanelStates(false, false, false, true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void SetPanelStates(
        bool showMain,
        bool showSettings,
        bool showCredits,
        bool showProgression
    )
    {
        if (mainPanel != null)
            mainPanel.SetActive(showMain);

        if (settingsPanel != null)
            settingsPanel.SetActive(showSettings);

        if (creditsPanel != null)
            creditsPanel.SetActive(showCredits);

        if (progressionPanel != null)
            progressionPanel.SetActive(showProgression);
    }
}
