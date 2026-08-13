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

    private bool ended;
    private Coroutine deathCheckRoutine;

    private void Awake()
    {
        if (playerDeathAnimation == null && playerHealth != null)
            playerDeathAnimation = playerHealth.GetComponent<PlayerDeathAnimation>();

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
            deathCheckRoutine = StartCoroutine(CheckDeathAfterRevive());
    }

    private IEnumerator CheckDeathAfterRevive()
    {
        yield return null;

        if (playerHealth == null || !playerHealth.IsDead)
        {
            deathCheckRoutine = null;
            yield break;
        }

        if (playerDeathAnimation != null)
            yield return playerDeathAnimation.WaitForFinalDeath();

        if (playerHealth != null && playerHealth.IsDead)
            ShowLose();

        deathCheckRoutine = null;
    }

    private void ShowWin()
    {
        Show("Run Complete", "You cleared the dungeon.", true);
    }

    private void ShowLose()
    {
        Show("Run Lost", "You were defeated.", false);
    }

    private void Show(string title, string body, bool won)
    {
        if (ended)
            return;

        ended = true;

        int creditsEarned = GrantStudyCredits(won);

        Time.timeScale = 0f;

        if (root != null)
            root.SetActive(true);

        if (titleText != null)
            titleText.text = title;

        if (bodyText != null)
        {
            bodyText.text = body +
                "\n\nRooms Cleared: " +
                (roomManager != null ? roomManager.ClearedRoomCount : 0) +
                "\nStudy Credits Earned: " +
                creditsEarned;
        }
    }

    private int GrantStudyCredits(bool won)
    {
        if (MetaProgressionManager.Instance == null || roomManager == null)
            return 0;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
