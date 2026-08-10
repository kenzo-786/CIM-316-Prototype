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
        {
            root.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
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

    public void Restart()
    {
        paused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void SetPaused(bool value)
    {
        if (paused == value)
        {
            return;
        }

        paused = value;

        if (paused)
        {
            HideGameplayUI();

            if (root != null)
            {
                root.SetActive(true);
                root.transform.SetAsLastSibling();
            }

            Time.timeScale = 0f;
        }
        else
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            RestoreGameplayUI();
            Time.timeScale = 1f;
        }
    }

    private void HideGameplayUI()
    {
        for (int i = 0; i < hideWhilePaused.Length; i++)
        {
            GameObject target = hideWhilePaused[i];

            if (target == null)
            {
                continue;
            }

            previousActiveStates[i] = target.activeSelf;
            target.SetActive(false);
        }
    }

    private void RestoreGameplayUI()
    {
        for (int i = 0; i < hideWhilePaused.Length; i++)
        {
            GameObject target = hideWhilePaused[i];

            if (target != null)
            {
                target.SetActive(previousActiveStates[i]);
            }
        }
    }
}
