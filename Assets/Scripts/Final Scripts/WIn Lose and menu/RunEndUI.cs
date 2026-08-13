using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RunEndUI : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerDeathAnimation playerDeathAnimation;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool ended;
    private Coroutine deathCheckRoutine;

    private void Awake()
    {
        if (playerDeathAnimation == null &&
            playerHealth != null)
        {
            playerDeathAnimation =
                playerHealth.GetComponent<PlayerDeathAnimation>();
        }

        if (root != null)
            root.SetActive(false);
    }

    private void OnEnable()
    {
        if (roomManager != null)
            roomManager.OnRunWon += ShowWin;

        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (roomManager != null)
            roomManager.OnRunWon -= ShowWin;

        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (deathCheckRoutine == null)
            deathCheckRoutine =
                StartCoroutine(CheckDeathAfterRevive());
    }

    private IEnumerator CheckDeathAfterRevive()
    {
        yield return null;

        if (playerHealth == null ||
            !playerHealth.IsDead)
        {
            deathCheckRoutine = null;
            yield break;
        }

        if (playerDeathAnimation != null)
        {
            yield return
                playerDeathAnimation.WaitForFinalDeath();
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            ShowLose();
        }

        deathCheckRoutine = null;
    }

    private void ShowWin()
    {
        Show(true);
    }

    private void ShowLose()
    {
        Show(false);
    }

    private void Show(bool won)
    {
        if (ended)
            return;

        ended = true;

        int creditsEarned = GrantStudyCredits(won);

        int totalCredits =
            MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.StudyCredits
                : 0;

        Time.timeScale = 0f;

        if (root != null)
            root.SetActive(true);

        if (titleText != null)
        {
            titleText.text = won
                ? "RUN COMPLETE"
                : "RUN LOST";
        }

        if (bodyText != null)
        {
            int roomsCleared =
                roomManager != null
                    ? roomManager.ClearedRoomCount
                    : 0;

            bodyText.text =
                (won
                    ? "The dungeon is cleared."
                    : "The deadline remains undefeated.") +
                "\n\nROOMS CLEARED\n" +
                roomsCleared +
                "\n\nSTUDY CREDITS EARNED\n+" +
                creditsEarned +
                "\n\nTOTAL STUDY CREDITS\n" +
                totalCredits;
        }
    }

    private int GrantStudyCredits(bool won)
    {
        if (MetaProgressionManager.Instance == null ||
            roomManager == null)
        {
            return 0;
        }

        return MetaProgressionManager.Instance.GrantRunRewards(
            roomManager.ClearedRoomCount,
            roomManager.EliteRoomsCleared,
            roomManager.BossRoomsCleared,
            roomManager.FinalRoomsCleared,
            won
        );
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
