using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashSceneController : MonoBehaviour
{
    [Header("Branding")]
    [SerializeField] private Sprite studioLogo;
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private Vector2 logoSize = new Vector2(1100f, 520f);

    [Header("Timing")]
    [SerializeField, Min(0f)] private float fadeInDuration = 1.1f;
    [SerializeField, Min(0f)] private float holdDuration = 2f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 1.1f;
    [SerializeField, Min(0f)] private float minimumDisplayTime = 1f;
    [SerializeField] private bool allowSkip = true;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainMenu";

    private CanvasGroup contentGroup;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        BuildPresentation();

        yield return Fade(0f, 1f, fadeInDuration);

        float elapsed = 0f;

        while (elapsed < holdDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (allowSkip &&
                elapsed >= minimumDisplayTime &&
                Input.anyKeyDown)
            {
                break;
            }

            yield return null;
        }

        yield return Fade(1f, 0f, fadeOutDuration);

        Cursor.visible = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void BuildPresentation()
    {
        GameObject canvasObject = new GameObject(
            "Splash Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(Image)
        );

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.SetParent(canvas.transform, false);
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundObject.GetComponent<Image>().color = backgroundColor;

        GameObject contentObject = new GameObject(
            "Logo Content",
            typeof(RectTransform),
            typeof(CanvasGroup)
        );

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.SetParent(backgroundRect, false);
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        contentGroup = contentObject.GetComponent<CanvasGroup>();
        contentGroup.alpha = 0f;

        GameObject logoObject = new GameObject(
            "Studio Logo",
            typeof(RectTransform),
            typeof(Image)
        );

        RectTransform logoRect = logoObject.GetComponent<RectTransform>();
        logoRect.SetParent(contentRect, false);
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.anchoredPosition = Vector2.zero;
        logoRect.sizeDelta = logoSize;

        Image logoImage = logoObject.GetComponent<Image>();
        logoImage.sprite = studioLogo;
        logoImage.preserveAspect = true;
        logoImage.raycastTarget = false;
        logoImage.enabled = studioLogo != null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (contentGroup == null)
            yield break;

        if (duration <= 0f)
        {
            contentGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            contentGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }

        contentGroup.alpha = to;
    }
}
