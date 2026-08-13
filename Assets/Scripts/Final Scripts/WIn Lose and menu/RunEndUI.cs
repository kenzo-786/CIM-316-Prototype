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

        EnsureTextReferences();
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

        EnsureTextReferences();

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

    private void EnsureTextReferences()
    {
        if (root == null)
            return;

        if (titleText == null)
            titleText = root.GetComponentInChildren<TMP_Text>(true);

        if (bodyText != null)
            return;

        GameObject bodyObject = new GameObject(
            "Run End Summary",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        bodyObject.transform.SetParent(root.transform, false);

        RectTransform rect = bodyObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(20f, -42f);
        rect.sizeDelta = new Vector2(640f, 360f);

        TextMeshProUGUI text = bodyObject.GetComponent<TextMeshProUGUI>();
        text.font = titleText != null && titleText.font != null
            ? titleText.font
            : TMP_Settings.defaultFontAsset;
        text.fontSize = 30f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = Color.white;
        text.raycastTarget = false;
        bodyText = text;
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
