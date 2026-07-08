using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Name")]
    //[SerializeField] private string gameScene = "GameScene";

    [Header("UI")]
    [SerializeField] private GameObject creditsPanel;

    void Start()
    {
        creditsPanel.SetActive(false);
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
