using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject menuContent;
    [SerializeField] private GameObject statusContent;
    [SerializeField] private RunStatusUI runStatusUI;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button runStatusButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button statusBackButton;

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

    private Dictionary<Button, Vector3> originalScales =
        new Dictionary<Button, Vector3>();

    private void Awake()
    {
        previousActiveStates =
            new bool[hideWhilePaused.Length];

        if (root != null)
            root.SetActive(false);

        RegisterButton(resumeButton);
        RegisterButton(runStatusButton);
        RegisterButton(restartButton);
        RegisterButton(quitButton);
        RegisterButton(statusBackButton);

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

        if (statusContent != null &&
            statusContent.activeSelf)
        {
            HandleStatusBackHover();

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

        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        Button newHoveredButton = null;

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button == resumeButton ||
                button == runStatusButton ||
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

    private void HandleStatusBackHover()
    {
        if (EventSystem.current == null ||
            statusBackButton == null)
            return;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        bool hoveringBack = false;

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button == statusBackButton)
            {
                hoveringBack = true;
                break;
            }
        }

        if (hoveringBack)
        {
            if (hoveredButton != statusBackButton)
            {
                hoveredButton = statusBackButton;
                PlayPop(statusBackButton);
            }
        }
        else if (hoveredButton == statusBackButton)
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
        {
            originalScales[button] =
                button.transform.localScale;
        }
    }

    private void SelectButton(Button button)
    {
        if (button == null)
            return;

        if (currentButton == button)
            return;

        currentButton = button;

        EventSystem.current.SetSelectedGameObject(
            button.gameObject
        );

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(true);

        UpdateSelectionIcon();
        PlayPop(button);
    }

    private void SelectNextButton()
    {
        if (currentButton == resumeButton)
        {
            SelectButton(runStatusButton);
        }
        else if (currentButton == runStatusButton)
        {
            SelectButton(restartButton);
        }
        else if (currentButton == restartButton)
        {
            SelectButton(quitButton);
        }
        else
        {
            SelectButton(resumeButton);
        }
    }

    private void SelectPreviousButton()
    {
        if (currentButton == resumeButton)
        {
            SelectButton(quitButton);
        }
        else if (currentButton == runStatusButton)
        {
            SelectButton(resumeButton);
        }
        else if (currentButton == restartButton)
        {
            SelectButton(runStatusButton);
        }
        else
        {
            SelectButton(restartButton);
        }
    }

    private void UpdateSelectionIcon()
    {
        if (currentButton == null ||
            selectionIcon == null)
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

        popCoroutine =
            StartCoroutine(PopButton(button));
    }

    private IEnumerator PopButton(Button button)
    {
        if (!originalScales.ContainsKey(button))
            yield break;

        Vector3 originalScale =
            originalScales[button];

        Vector3 enlargedScale =
            originalScale * popScale;

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

        button.transform.localScale =
            originalScale;
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

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(false);

        currentButton = null;
        hoveredButton = null;

        EventSystem.current.SetSelectedGameObject(null);

        if (runStatusUI != null)
            runStatusUI.Refresh();
    }

    public void ShowMenuContent()
    {
        if (menuContent != null)
            menuContent.SetActive(true);

        if (statusContent != null)
            statusContent.SetActive(false);

        currentButton = resumeButton;
        hoveredButton = null;

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(
            resumeButton.gameObject
        );

        UpdateSelectionIcon();
    }

    public void CloseStatus()
    {
        PlayPop(statusBackButton);
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

    private void SetPaused(bool value)
    {
        paused = value;

        if (paused)
        {
            for (int i = 0;
                 i < hideWhilePaused.Length;
                 i++)
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
            for (int i = 0;
                 i < hideWhilePaused.Length;
                 i++)
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