using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject menuContent;
    [SerializeField] private GameObject statusContent;
    [SerializeField] private GameObject settingsContent;
    [SerializeField] private RunStatusUI runStatusUI;
    [SerializeField] private SettingsMenuBinder settingsMenuBinder;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button runStatusButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button statusBackButton;
    [SerializeField] private Button settingsBackButton;

    [Header("Selection Icon")]
    [SerializeField] private RectTransform selectionIcon;
    [SerializeField] private float iconDistance = 75f;

    [Header("Button Pop")]
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    [Header("Pause")]
    [SerializeField] private GameObject[] hideWhilePaused;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool[] previousActiveStates;
    private bool paused;
    private Button currentButton;
    private Button hoveredButton;
    private Coroutine popCoroutine;

    private readonly Dictionary<Button, Vector3> originalScales =
        new Dictionary<Button, Vector3>();

    private void Awake()
    {
        previousActiveStates = new bool[hideWhilePaused.Length];

        if (root != null)
            root.SetActive(false);

        RegisterButton(resumeButton);
        RegisterButton(runStatusButton);
        RegisterButton(settingsButton);
        RegisterButton(restartButton);
        RegisterButton(quitButton);
        RegisterButton(statusBackButton);
        RegisterButton(settingsBackButton);

        ShowMenuContent();

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!paused)
        {
            if (Input.GetKeyDown(pauseKey))
                SetPaused(true);

            return;
        }

        if (statusContent != null && statusContent.activeSelf)
        {
            HandleSubmenuBackHover(statusBackButton);

            if (Input.GetKeyDown(pauseKey))
                ShowMenuContent();

            return;
        }

        if (settingsContent != null && settingsContent.activeSelf)
        {
            HandleSubmenuBackHover(settingsBackButton);

            if (Input.GetKeyDown(pauseKey))
                ShowMenuContent();

            return;
        }

        HandleMouseHover();
        HandleKeyboardNavigation();

        if (Input.GetKeyDown(pauseKey))
            SetPaused(false);
    }

    private void HandleMouseHover()
    {
        if (EventSystem.current == null)
            return;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        Button newHoveredButton = null;

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button == resumeButton ||
                button == runStatusButton ||
                button == settingsButton ||
                button == restartButton ||
                button == quitButton)
            {
                newHoveredButton = button;
                break;
            }
        }

        if (newHoveredButton != null &&
            newHoveredButton != currentButton)
        {
            hoveredButton = newHoveredButton;
            SelectButton(newHoveredButton);
        }
        else if (newHoveredButton == null)
        {
            hoveredButton = null;
        }
    }

    private void HandleSubmenuBackHover(Button backButton)
    {
        if (EventSystem.current == null || backButton == null)
            return;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        bool hoveringBack = false;

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button == backButton)
            {
                hoveringBack = true;
                break;
            }
        }

        if (hoveringBack && hoveredButton != backButton)
        {
            hoveredButton = backButton;
            PlayPop(backButton);
        }
        else if (!hoveringBack && hoveredButton == backButton)
        {
            hoveredButton = null;
        }
    }

    private void HandleKeyboardNavigation()
    {
        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectPreviousButton();
        }

        if (Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectNextButton();
        }
    }

    private void RegisterButton(Button button)
    {
        if (button != null)
            originalScales[button] = button.transform.localScale;
    }

    private void SelectButton(Button button)
    {
        if (button == null || currentButton == button)
            return;

        currentButton = button;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button.gameObject);

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(true);

        UpdateSelectionIcon();
        PlayPop(button);
    }

    private void SelectNextButton()
    {
        if (currentButton == resumeButton)
            SelectButton(runStatusButton);
        else if (currentButton == runStatusButton)
            SelectButton(settingsButton);
        else if (currentButton == settingsButton)
            SelectButton(restartButton);
        else if (currentButton == restartButton)
            SelectButton(quitButton);
        else
            SelectButton(resumeButton);
    }

    private void SelectPreviousButton()
    {
        if (currentButton == resumeButton)
            SelectButton(quitButton);
        else if (currentButton == runStatusButton)
            SelectButton(resumeButton);
        else if (currentButton == settingsButton)
            SelectButton(runStatusButton);
        else if (currentButton == restartButton)
            SelectButton(settingsButton);
        else
            SelectButton(restartButton);
    }

    private void UpdateSelectionIcon()
    {
        if (currentButton == null || selectionIcon == null)
            return;

        RectTransform buttonRect =
            currentButton.GetComponent<RectTransform>();

        selectionIcon.position = new Vector3(
            buttonRect.position.x -
            buttonRect.rect.width / 2f -
            iconDistance,
            buttonRect.position.y,
            selectionIcon.position.z
        );
    }

    private void PlayPop(Button button)
    {
        if (button == null)
            return;

        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopButton(button));
    }

    private IEnumerator PopButton(Button button)
    {
        if (!originalScales.ContainsKey(button))
            yield break;

        Vector3 originalScale = originalScales[button];
        Vector3 enlargedScale = originalScale * popScale;

        float time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            button.transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    time / popSpeed
                );

            yield return null;
        }

        time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            button.transform.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    time / popSpeed
                );

            yield return null;
        }

        button.transform.localScale = originalScale;
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

        if (settingsContent != null)
            settingsContent.SetActive(false);

        if (statusContent != null)
            statusContent.SetActive(true);

        HideSelection();

        if (runStatusUI != null)
            runStatusUI.Refresh();
    }

    public void ShowSettings()
    {
        if (!paused)
            return;

        if (menuContent != null)
            menuContent.SetActive(false);

        if (statusContent != null)
            statusContent.SetActive(false);

        if (settingsContent != null)
            settingsContent.SetActive(true);

        HideSelection();

        if (settingsMenuBinder != null)
            settingsMenuBinder.Refresh();
    }

    public void ShowMenuContent()
    {
        if (menuContent != null)
            menuContent.SetActive(true);

        if (statusContent != null)
            statusContent.SetActive(false);

        if (settingsContent != null)
            settingsContent.SetActive(false);

        currentButton = resumeButton;
        hoveredButton = null;

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(true);

        if (EventSystem.current != null &&
            resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(
                resumeButton.gameObject
            );
        }

        UpdateSelectionIcon();
    }

    public void CloseStatus()
    {
        PlayPop(statusBackButton);
        ShowMenuContent();
    }

    public void CloseSettings()
    {
        PlayPop(settingsBackButton);
        ShowMenuContent();
    }

    public void Restart()
    {
        SetPaused(false);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
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

    private void HideSelection()
    {
        currentButton = null;
        hoveredButton = null;

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
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

            if (selectionIcon != null)
                selectionIcon.gameObject.SetActive(false);

            currentButton = null;
            hoveredButton = null;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (root != null)
                root.SetActive(false);
        }

        if (root != null)
            root.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        GameAudioManager.Instance?.SetPaused(paused);
    }
}