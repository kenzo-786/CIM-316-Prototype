using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Button upgradesBackButton;

    [Header("Selection Icon")]
    [SerializeField] private RectTransform selectionIcon;
    [SerializeField] private float iconDistance = 75f;

    [Header("UI Panels")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject upgradesPanel;

    [Header("Credits Back Button")]
    [SerializeField] private Color creditsBackNormalColor = Color.white;
    [SerializeField] private Color creditsBackHoverColor = Color.blue;
    
    [Header("Upgrades Back Button")]
    [SerializeField] private Color upgradesBackNormalColor = Color.white;
    [SerializeField] private Color upgradesBackHoverColor = Color.blue;

    [Header("Button Pop")]
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    private Button currentButton;
    private Button hoveredButton;
    private Coroutine popCoroutine;

    private Dictionary<Button, Vector3> originalScales =
        new Dictionary<Button, Vector3>();

    void Start()
    {
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        upgradesPanel.SetActive(false);

        RegisterButton(playButton);
        RegisterButton(optionsButton);
        RegisterButton(creditsButton);
        RegisterButton(upgradesButton);
        RegisterButton(quitButton);
        RegisterButton(creditsBackButton);
        RegisterButton(upgradesBackButton);

        currentButton = playButton;
        hoveredButton = null;

        EventSystem.current.SetSelectedGameObject(
            playButton.gameObject
        );

        selectionIcon.gameObject.SetActive(true);

        UpdateSelectionIcon();
    }

    void Update()
    {
        if (creditsPanel != null && creditsPanel.activeSelf)
        {
            HandleCreditsBackHover();
            return;
        }

        if (upgradesPanel != null && upgradesPanel.activeSelf)
        {
            HandleUpgradesBackHover();
            return;
        }

        HandleMouseHover();
        HandleKeyboardNavigation();
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

            if (button == playButton ||
                button == optionsButton ||
                button == creditsButton ||
                button == upgradesButton ||
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

        selectionIcon.gameObject.SetActive(true);

        UpdateSelectionIcon();
        PlayPop(button);
    }

    private void SelectNextButton()
    {
        if (currentButton == playButton)
        {
            SelectButton(optionsButton);
        }
        else if (currentButton == optionsButton)
        {
            SelectButton(creditsButton);
        }
        else if (currentButton == creditsButton)
        {
            SelectButton(upgradesButton);
        }
        else if (currentButton == upgradesButton)
        {
            SelectButton(quitButton);
        }
        else
        {
            SelectButton(playButton);
        }
    }

    private void SelectPreviousButton()
    {
        if (currentButton == playButton)
        {
            SelectButton(quitButton);
        }
        else if (currentButton == optionsButton)
        {
            SelectButton(playButton);
        }
        else if (currentButton == creditsButton)
        {
            SelectButton(optionsButton);
        }
        else if (currentButton == upgradesButton)
        {
            SelectButton(creditsButton);
        }
        else
        {
            SelectButton(upgradesButton);
        }
    }

    private void UpdateSelectionIcon()
    {
        if (currentButton == null)
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

    private void HandleCreditsBackHover()
    {
        if (EventSystem.current == null ||
            creditsBackButton == null)
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

            if (button == creditsBackButton)
            {
                hoveringBack = true;
                break;
            }
        }

        if (hoveringBack)
        {
            if (hoveredButton != creditsBackButton)
            {
                hoveredButton = creditsBackButton;

                SetCreditsBackColor(
                    creditsBackHoverColor
                );

                PlayPop(creditsBackButton);
            }
        }
        else if (hoveredButton == creditsBackButton)
        {
            hoveredButton = null;

            SetCreditsBackColor(
                creditsBackNormalColor
            );
        }
    }

    private void HandleUpgradesBackHover()
    {
        if (EventSystem.current == null ||
            upgradesBackButton == null)
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

            if (button == upgradesBackButton)
            {
                hoveringBack = true;
                break;
            }
        }

        if (hoveringBack)
        {
            if (hoveredButton != upgradesBackButton)
            {
                hoveredButton = upgradesBackButton;

                SetUpgradesBackColor(
                    upgradesBackHoverColor
                );

                PlayPop(upgradesBackButton);
            }
        }
        else if (hoveredButton == upgradesBackButton)
        {
            hoveredButton = null;

            SetUpgradesBackColor(
                upgradesBackNormalColor
            );
        }
    }

    private void SetCreditsBackColor(Color color)
    {
        if (creditsBackButton == null)
            return;

        ColorBlock colors =
            creditsBackButton.colors;

        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.pressedColor = color;

        creditsBackButton.colors = colors;
    }
    
    private void SetUpgradesBackColor(Color color)
    {
        if (upgradesBackButton == null)
            return;

        ColorBlock colors =
            upgradesBackButton.colors;

        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.pressedColor = color;

        upgradesBackButton.colors = colors;
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

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);

        currentButton = null;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(false);

        SetCreditsBackColor(
            creditsBackNormalColor
        );

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseCredits()
    {
        PlayPop(creditsBackButton);

        creditsPanel.SetActive(false);

        currentButton = playButton;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(
            playButton.gameObject
        );

        UpdateSelectionIcon();

        SetCreditsBackColor(
            creditsBackNormalColor
        );
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);

        currentButton = null;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);

        currentButton = playButton;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(
            playButton.gameObject
        );

        UpdateSelectionIcon();
    }

    public void OpenUpgrades()
    {
        upgradesPanel.SetActive(true);

        currentButton = null;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseUpgrades()
    {
        PlayPop(upgradesBackButton);

        upgradesPanel.SetActive(false);

        currentButton = playButton;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(
            playButton.gameObject
        );

        UpdateSelectionIcon();

        SetCreditsBackColor(
            upgradesBackNormalColor
        );
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