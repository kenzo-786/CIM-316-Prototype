using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RunEndUI : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    private bool ended;

    private void Awake()
    {
        root.SetActive(false);
    }

    private void OnEnable()
    {
        roomManager.OnRunWon += ShowWin;
        playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        roomManager.OnRunWon -= ShowWin;
        playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        StartCoroutine(CheckDeathAfterRevive());
    }

    private IEnumerator CheckDeathAfterRevive()
    {
        yield return null;

        if (playerHealth.IsDead)
            ShowLose();
    }

    private void ShowWin()
    {
        Show("Prototype Complete", "You cleared the run.");
    }

    private void ShowLose()
    {
        Show("Run Lost", "You were defeated.");
    }

    private void Show(string title, string body)
    {
        if (ended) return;

        ended = true;
        Time.timeScale = 0f;
        root.SetActive(true);

        titleText.text = title;
        bodyText.text = body;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
