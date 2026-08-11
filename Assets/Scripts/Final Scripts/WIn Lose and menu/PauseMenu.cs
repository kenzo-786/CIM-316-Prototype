using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject[] hideWhilePaused;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool[] previousActiveStates;
    private bool paused;

    private void Awake()
    {
        previousActiveStates = new bool[hideWhilePaused.Length];

        if (root != null)
            root.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    public void TogglePause()
    {
        SetPaused(!paused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        SetPaused(false);
        Application.Quit();
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

                previousActiveStates[i] = hideWhilePaused[i].activeSelf;
                hideWhilePaused[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < hideWhilePaused.Length; i++)
            {
                if (hideWhilePaused[i] != null && previousActiveStates[i])
                    hideWhilePaused[i].SetActive(true);
            }
        }

        if (root != null)
            root.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
        GameAudioManager.Instance?.SetPaused(paused);
    }
}
