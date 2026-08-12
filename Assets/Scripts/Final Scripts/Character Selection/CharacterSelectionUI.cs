using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject boyPanel;
    [SerializeField] private GameObject girlPanel;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button canvasBackButton;

    [Header("Character Portraits")]
    [SerializeField] private RectTransform boyPortrait;
    [SerializeField] private RectTransform girlPortrait;

    [Header("Main Menu Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Button Pop")]
    [SerializeField] private float buttonPopScale = 1.05f;
    [SerializeField] private float buttonPopSpeed = 0.08f;

    [Header("Portrait Pop")]
    [SerializeField] private float portraitPopScale = 1.12f;
    [SerializeField] private float portraitPopSpeed = 0.08f;

    private Vector3 boyOriginalScale;
    private Vector3 girlOriginalScale;
    private Vector3 nextOriginalScale;
    private Vector3 backOriginalScale;
    private Vector3 canvasBackOriginalScale;

    private Coroutine boyPopCoroutine;
    private Coroutine girlPopCoroutine;
    private Coroutine nextPopCoroutine;
    private Coroutine backPopCoroutine;
    private Coroutine canvasBackPopCoroutine;

    private bool nextButtonHovered;
    private bool backButtonHovered;
    private bool canvasBackButtonHovered;

    private void Awake()
    {
        if (boyPortrait != null)
            boyOriginalScale = boyPortrait.localScale;

        if (girlPortrait != null)
            girlOriginalScale = girlPortrait.localScale;

        if (nextButton != null)
            nextOriginalScale = nextButton.transform.localScale;

        if (backButton != null)
            backOriginalScale = backButton.transform.localScale;

        if (canvasBackButton != null)
            canvasBackOriginalScale = canvasBackButton.transform.localScale;

        if (nextButton != null)
            nextButton.onClick.AddListener(OpenGirlPanel);

        if (backButton != null)
            backButton.onClick.AddListener(OpenBoyPanel);

        if (canvasBackButton != null)
            canvasBackButton.onClick.AddListener(BackToMainMenu);

        ShowBoyPanel();
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OpenGirlPanel);

        if (backButton != null)
            backButton.onClick.RemoveListener(OpenBoyPanel);

        if (canvasBackButton != null)
            canvasBackButton.onClick.RemoveListener(BackToMainMenu);
    }

    private void Update()
    {
        HandlePortraitHover();
        HandleButtonHover();
    }

    private void HandlePortraitHover()
    {
        if (boyPanel != null &&
            boyPanel.activeSelf &&
            boyPortrait != null)
        {
            if (IsPointerOver(boyPortrait))
            {
                if (boyPopCoroutine == null)
                {
                    boyPopCoroutine = StartCoroutine(
                        PopPortrait(
                            boyPortrait,
                            boyOriginalScale,
                            true
                        )
                    );
                }
            }
            else
            {
                if (boyPopCoroutine != null)
                {
                    StopCoroutine(boyPopCoroutine);
                    boyPopCoroutine = null;
                }

                boyPortrait.localScale = boyOriginalScale;
            }
        }

        if (girlPanel != null &&
            girlPanel.activeSelf &&
            girlPortrait != null)
        {
            if (IsPointerOver(girlPortrait))
            {
                if (girlPopCoroutine == null)
                {
                    girlPopCoroutine = StartCoroutine(
                        PopPortrait(
                            girlPortrait,
                            girlOriginalScale,
                            false
                        )
                    );
                }
            }
            else
            {
                if (girlPopCoroutine != null)
                {
                    StopCoroutine(girlPopCoroutine);
                    girlPopCoroutine = null;
                }

                girlPortrait.localScale = girlOriginalScale;
            }
        }
    }

    private void HandleButtonHover()
    {
        if (nextButton != null)
        {
            bool isHoveringNext =
                IsPointerOver(nextButton.GetComponent<RectTransform>());

            if (isHoveringNext && !nextButtonHovered)
            {
                nextButtonHovered = true;

                if (nextPopCoroutine != null)
                    StopCoroutine(nextPopCoroutine);

                nextPopCoroutine = StartCoroutine(
                    PopButton(
                        nextButton.transform,
                        nextOriginalScale
                    )
                );
            }
            else if (!isHoveringNext && nextButtonHovered)
            {
                nextButtonHovered = false;

                if (nextPopCoroutine != null)
                    StopCoroutine(nextPopCoroutine);

                nextPopCoroutine = StartCoroutine(
                    ReturnButtonToOriginal(
                        nextButton.transform,
                        nextOriginalScale
                    )
                );
            }
        }

        if (backButton != null)
        {
            bool isHoveringBack =
                IsPointerOver(backButton.GetComponent<RectTransform>());

            if (isHoveringBack && !backButtonHovered)
            {
                backButtonHovered = true;

                if (backPopCoroutine != null)
                    StopCoroutine(backPopCoroutine);

                backPopCoroutine = StartCoroutine(
                    PopButton(
                        backButton.transform,
                        backOriginalScale
                    )
                );
            }
            else if (!isHoveringBack && backButtonHovered)
            {
                backButtonHovered = false;

                if (backPopCoroutine != null)
                    StopCoroutine(backPopCoroutine);

                backPopCoroutine = StartCoroutine(
                    ReturnButtonToOriginal(
                        backButton.transform,
                        backOriginalScale
                    )
                );
            }
        }

        if (canvasBackButton != null)
        {
            bool isHoveringCanvasBack =
                IsPointerOver(
                    canvasBackButton.GetComponent<RectTransform>()
                );

            if (isHoveringCanvasBack && !canvasBackButtonHovered)
            {
                canvasBackButtonHovered = true;

                if (canvasBackPopCoroutine != null)
                    StopCoroutine(canvasBackPopCoroutine);

                canvasBackPopCoroutine = StartCoroutine(
                    PopButton(
                        canvasBackButton.transform,
                        canvasBackOriginalScale
                    )
                );
            }
            else if (!isHoveringCanvasBack && canvasBackButtonHovered)
            {
                canvasBackButtonHovered = false;

                if (canvasBackPopCoroutine != null)
                    StopCoroutine(canvasBackPopCoroutine);

                canvasBackPopCoroutine = StartCoroutine(
                    ReturnButtonToOriginal(
                        canvasBackButton.transform,
                        canvasBackOriginalScale
                    )
                );
            }
        }
    }

    private bool IsPointerOver(RectTransform target)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            target,
            Input.mousePosition,
            null
        );
    }

    private void OpenGirlPanel()
    {
        if (boyPanel != null)
            boyPanel.SetActive(false);

        if (girlPanel != null)
            girlPanel.SetActive(true);
    }

    private void OpenBoyPanel()
    {
        if (girlPanel != null)
            girlPanel.SetActive(false);

        if (boyPanel != null)
            boyPanel.SetActive(true);
    }

    private void ShowBoyPanel()
    {
        if (boyPanel != null)
            boyPanel.SetActive(true);

        if (girlPanel != null)
            girlPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator PopButton(
        Transform target,
        Vector3 originalScale
    )
    {
        Vector3 enlargedScale =
            originalScale * buttonPopScale;

        Vector3 startingScale = target.localScale;

        float time = 0f;

        while (time < buttonPopSpeed)
        {
            time += Time.unscaledDeltaTime;

            target.localScale = Vector3.Lerp(
                startingScale,
                enlargedScale,
                time / buttonPopSpeed
            );

            yield return null;
        }

        target.localScale = enlargedScale;
    }

    private IEnumerator ReturnButtonToOriginal(
        Transform target,
        Vector3 originalScale
    )
    {
        Vector3 startingScale = target.localScale;

        float time = 0f;

        while (time < buttonPopSpeed)
        {
            time += Time.unscaledDeltaTime;

            target.localScale = Vector3.Lerp(
                startingScale,
                originalScale,
                time / buttonPopSpeed
            );

            yield return null;
        }

        target.localScale = originalScale;
    }

    private IEnumerator PopPortrait(
        RectTransform portrait,
        Vector3 originalScale,
        bool isBoy
    )
    {
        Vector3 enlargedScale =
            originalScale * portraitPopScale;

        Vector3 startingScale = portrait.localScale;

        float time = 0f;

        while (time < portraitPopSpeed)
        {
            time += Time.unscaledDeltaTime;

            portrait.localScale = Vector3.Lerp(
                startingScale,
                enlargedScale,
                time / portraitPopSpeed
            );

            yield return null;
        }

        portrait.localScale = enlargedScale;

        while (true)
        {
            if (isBoy)
            {
                if (boyPanel == null ||
                    !boyPanel.activeSelf ||
                    !IsPointerOver(boyPortrait))
                {
                    break;
                }
            }
            else
            {
                if (girlPanel == null ||
                    !girlPanel.activeSelf ||
                    !IsPointerOver(girlPortrait))
                {
                    break;
                }
            }

            yield return null;
        }

        time = 0f;

        startingScale = portrait.localScale;

        while (time < portraitPopSpeed)
        {
            time += Time.unscaledDeltaTime;

            portrait.localScale = Vector3.Lerp(
                startingScale,
                originalScale,
                time / portraitPopSpeed
            );

            yield return null;
        }

        portrait.localScale = originalScale;

        if (isBoy)
            boyPopCoroutine = null;
        else
            girlPopCoroutine = null;
    }
}