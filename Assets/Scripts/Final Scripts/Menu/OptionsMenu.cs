using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button volumeButton;

    [Header("Selection Icon")]
    [SerializeField] private RectTransform selectionIcon;
    [SerializeField] private float iconGap = 12f;

    [Header("Content")]
    [SerializeField] private GameObject controlsInfo;
    [SerializeField] private GameObject volumeContent;

    [Header("Content Animation")]
    [SerializeField] private float contentMoveDistance = 40f;
    [SerializeField] private float contentAnimationSpeed = 10f;

    [Header("Button Pop")]
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    private Button currentButton;
    private Button hoveredButton;

    private Coroutine popCoroutine;
    private Coroutine contentCoroutine;

    private Dictionary<Button, Vector3> originalScales =
        new Dictionary<Button, Vector3>();

    private RectTransform controlsRect;
    private RectTransform volumeRect;

    private Vector2 controlsOriginalPosition;
    private Vector2 volumeOriginalPosition;

    private void Awake()
    {
        if (controlsInfo != null)
        {
            controlsRect = controlsInfo.GetComponent<RectTransform>();
            controlsOriginalPosition = controlsRect.anchoredPosition;
        }

        if (volumeContent != null)
        {
            volumeRect = volumeContent.GetComponent<RectTransform>();
            volumeOriginalPosition = volumeRect.anchoredPosition;
        }
    }

    private void Start()
    {
        RegisterButton(controlsButton);
        RegisterButton(volumeButton);

        currentButton = null;
        hoveredButton = null;

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(false);

        if (controlsInfo != null)
            controlsInfo.SetActive(false);

        if (volumeContent != null)
            volumeContent.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void Update()
    {
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

        EventSystem.current.RaycastAll(pointerData, results);

        Button newHoveredButton = null;

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button == controlsButton ||
                button == volumeButton)
            {
                newHoveredButton = button;
                break;
            }
        }

        if (newHoveredButton != hoveredButton)
        {
            hoveredButton = newHoveredButton;

            if (hoveredButton != null)
                SelectButton(hoveredButton);
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

    private void SelectNextButton()
    {
        if (currentButton == null)
        {
            SelectButton(controlsButton);
            return;
        }

        if (currentButton == controlsButton)
            SelectButton(volumeButton);
        else
            SelectButton(controlsButton);
    }

    private void SelectPreviousButton()
    {
        if (currentButton == null)
        {
            SelectButton(controlsButton);
            return;
        }

        if (currentButton == controlsButton)
            SelectButton(volumeButton);
        else
            SelectButton(controlsButton);
    }

    private void RegisterButton(Button button)
    {
        if (button == null)
            return;

        originalScales[button] =
            button.transform.localScale;
    }

    private void SelectButton(Button button)
    {
        if (button == null)
            return;

        if (currentButton == button)
            return;

        currentButton = button;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                button.gameObject
            );
        }

        UpdateSelectionIcon();
        UpdateContent();
        PlayPop(button);
    }

    private void UpdateSelectionIcon()
    {
        if (currentButton == null ||
            selectionIcon == null)
            return;

        RectTransform buttonRect =
            currentButton.GetComponent<RectTransform>();

        if (buttonRect == null)
            return;

        selectionIcon.gameObject.SetActive(true);

        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);

        float leftX = corners[0].x;
        float centerY = (corners[0].y + corners[1].y) / 2f;

        Vector3 iconPosition = selectionIcon.position;

        iconPosition.x = leftX - iconGap;
        iconPosition.y = centerY;

        selectionIcon.position = iconPosition;
    }

    private void UpdateContent()
    {
        if (currentButton == controlsButton)
            ShowControls();
        else if (currentButton == volumeButton)
            ShowVolume();
    }

    private void ShowControls()
    {
        StopContentAnimation();

        if (controlsInfo != null)
        {
            controlsInfo.SetActive(true);

            controlsRect.anchoredPosition =
                controlsOriginalPosition +
                Vector2.right * contentMoveDistance;

            contentCoroutine =
                StartCoroutine(
                    AnimateContent(
                        controlsRect,
                        controlsOriginalPosition
                    )
                );
        }

        if (volumeContent != null)
            volumeContent.SetActive(false);
    }

    private void ShowVolume()
    {
        StopContentAnimation();

        if (controlsInfo != null)
            controlsInfo.SetActive(false);

        if (volumeContent != null)
        {
            volumeContent.SetActive(true);

            volumeRect.anchoredPosition =
                volumeOriginalPosition +
                Vector2.right * contentMoveDistance;

            contentCoroutine =
                StartCoroutine(
                    AnimateContent(
                        volumeRect,
                        volumeOriginalPosition
                    )
                );
        }
    }

    private IEnumerator AnimateContent(
        RectTransform content,
        Vector2 targetPosition)
    {
        Vector2 startPosition =
            content.anchoredPosition;

        float time = 0f;

        while (time < 1f)
        {
            time +=
                Time.unscaledDeltaTime *
                contentAnimationSpeed;

            float t =
                Mathf.SmoothStep(0f, 1f, time);

            content.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        content.anchoredPosition = targetPosition;
        contentCoroutine = null;
    }

    private void StopContentAnimation()
    {
        if (contentCoroutine != null)
        {
            StopCoroutine(contentCoroutine);
            contentCoroutine = null;
        }
    }

    private void PlayPop(Button button)
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine =
            StartCoroutine(
                PopButton(button)
            );
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

        button.transform.localScale = originalScale;
        popCoroutine = null;
    }

    public void OpenOptions()
    {
        gameObject.SetActive(true);

        currentButton = null;
        hoveredButton = null;

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(false);

        if (controlsInfo != null)
            controlsInfo.SetActive(false);

        if (volumeContent != null)
            volumeContent.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseOptions()
    {
        currentButton = null;
        hoveredButton = null;

        StopContentAnimation();

        if (selectionIcon != null)
            selectionIcon.gameObject.SetActive(false);

        if (controlsInfo != null)
            controlsInfo.SetActive(false);

        if (volumeContent != null)
            volumeContent.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        gameObject.SetActive(false);
    }
}