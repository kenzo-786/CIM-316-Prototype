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
    [SerializeField] private float iconDistance = 75f;

    [Header("Content")]
    [SerializeField] private GameObject controlsInfo;
    [SerializeField] private GameObject volumeSlider;

    [Header("Button Pop")]
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    private Button currentButton;
    private Button hoveredButton;
    private Coroutine popCoroutine;

    private Dictionary<Button, Vector3> originalScales = new Dictionary<Button, Vector3>();

    void Start()
    {
        RegisterButton(controlsButton);
        RegisterButton(volumeButton);

        currentButton = null;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(false);

        controlsInfo.SetActive(false);
        volumeSlider.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    void Update()
    {
        HandleMouseHover();
        HandleKeyboardNavigation();
    }

    private void HandleMouseHover()
    {
        if (EventSystem.current == null)
            return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Button newHoveredButton = null;

        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponentInParent<Button>();

            if (button == controlsButton || button == volumeButton)
            {
                newHoveredButton = button;
                break;
            }
        }

        if (newHoveredButton != hoveredButton)
        {
            hoveredButton = newHoveredButton;

            if (hoveredButton != null)
            {
                SelectButton(hoveredButton);
            }
            else
            {
                selectionIcon.gameObject.SetActive(false);
            }
        }
    }

    private void HandleKeyboardNavigation()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectPreviousButton();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectNextButton();
        }
    }

    private void RegisterButton(Button button)
    {
        if (button != null)
        {
            originalScales[button] = button.transform.localScale;
        }
    }

    private void SelectButton(Button button)
    {
        if (button == null)
            return;

        if (currentButton == button)
            return;

        currentButton = button;

        EventSystem.current.SetSelectedGameObject(button.gameObject);

        selectionIcon.gameObject.SetActive(true);

        UpdateSelectionIcon();
        UpdateContent();
        PlayPop(button);
    }

    private void SelectNextButton()
    {
        if (currentButton == null)
        {
            SelectButton(controlsButton);
        }
        else if (currentButton == controlsButton)
        {
            SelectButton(volumeButton);
        }
        else
        {
            SelectButton(controlsButton);
        }
    }

    private void SelectPreviousButton()
    {
        if (currentButton == null)
        {
            SelectButton(controlsButton);
        }
        else if (currentButton == controlsButton)
        {
            SelectButton(volumeButton);
        }
        else
        {
            SelectButton(controlsButton);
        }
    }

    private void UpdateSelectionIcon()
    {
        if (currentButton == null)
            return;

        RectTransform buttonRect = currentButton.GetComponent<RectTransform>();

        selectionIcon.position = new Vector3(
            buttonRect.position.x - buttonRect.rect.width / 1f - iconDistance,
            buttonRect.position.y,
            selectionIcon.position.z
        );
    }

    private void UpdateContent()
    {
        if (currentButton == controlsButton)
        {
            controlsInfo.SetActive(true);
            volumeSlider.SetActive(false);
        }
        else if (currentButton == volumeButton)
        {
            controlsInfo.SetActive(false);
            volumeSlider.SetActive(true);
        }
    }

    private void PlayPop(Button button)
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopButton(button));
    }

    private IEnumerator PopButton(Button button)
    {
        Vector3 originalScale = originalScales[button];
        Vector3 enlargedScale = originalScale * popScale;

        float time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            button.transform.localScale = Vector3.Lerp(
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

            button.transform.localScale = Vector3.Lerp(
                enlargedScale,
                originalScale,
                time / popSpeed
            );

            yield return null;
        }

        button.transform.localScale = originalScale;
    }

    public void CloseOptions()
    {
        currentButton = null;
        hoveredButton = null;

        selectionIcon.gameObject.SetActive(false);

        controlsInfo.SetActive(false);
        volumeSlider.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }
}