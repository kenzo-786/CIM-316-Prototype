using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject menuContent;
    [SerializeField] private GameObject statusContent;
    [SerializeField] private RunStatusUI runStatusUI;
    [SerializeField] private GameObject[] hideWhilePaused;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool[] previousActiveStates;
    private bool paused;

    private void Awake()
    {
        previousActiveStates = new bool[hideWhilePaused.Length];

        if (root != null)
            root.SetActive(false);

        ShowMenuContent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (!paused)
            {
                SetPaused(true);
            }
            else if (statusContent != null && statusContent.activeSelf)
            {
                ShowMenuContent();
            }
            else
            {
                SetPaused(false);
            }
        }
    }

    public void TogglePause()
    {
        SetPaused(!paused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void ShowStatus()
    {
        if (!paused)
            return;

        if (menuContent != null)
            menuContent.SetActive(false);

        if (statusContent != null)
            statusContent.SetActive(true);

        if (runStatusUI != null)
            runStatusUI.Refresh();
    }

    public void ShowMenuContent()
    {
        if (menuContent != null)
            menuContent.SetActive(true);

        if (statusContent != null)
            statusContent.SetActive(false);
    }

    public void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        SetPaused(false);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPaused(bool value)
    {
        paused = value;

        if (paused)
        {
            for (int i = 0; i < hideWhilePaused.Length; i++)
            {
                if (hideWhilePaused[i] == null)
                    continue;

                previousActiveStates[i] =
                    hideWhilePaused[i].activeSelf;

                hideWhilePaused[i].SetActive(false);
            }

            ShowMenuContent();
        }
        else
        {
            for (int i = 0; i < hideWhilePaused.Length; i++)
            {
                if (hideWhilePaused[i] != null &&
                    previousActiveStates[i])
                {
                    hideWhilePaused[i].SetActive(true);
                }
            }

            ShowMenuContent();
        }

        if (root != null)
            root.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        GameAudioManager.Instance?.SetPaused(paused);
    }
}