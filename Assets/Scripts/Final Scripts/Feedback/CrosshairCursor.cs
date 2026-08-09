using UnityEngine;
using UnityEngine.UI;

public class CrosshairCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private PlayerWeaponController
        weaponController;

    [Header("UI Screens That Need Normal Cursor")]
    [SerializeField]
    private GameObject[] uiScreens;

    [Header("Crosshair")]
    [SerializeField]
    private Color color = Color.white;

    [SerializeField]
    private float gap = 6f;

    [SerializeField]
    private float length = 12f;

    [SerializeField]
    private float thickness = 2f;

    [SerializeField]
    private Vector2 offset;

    [Header("Cursor")]
    [SerializeField]
    private bool
        hideSystemCursorDuringGameplay = true;

    private RectTransform crosshairRoot;
    private bool usingUiCursor;
    private bool showingCrosshair;

    private void Awake()
    {
        if (canvas == null)
        {
            canvas =
                GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas =
                FindObjectOfType<Canvas>();
        }

        if (canvas == null)
            canvas = CreateCrosshairCanvas();

        if (weaponController == null)
        {
            weaponController =
                FindObjectOfType
                    <PlayerWeaponController>();
        }

        BuildCrosshair();
    }

    private void OnEnable()
    {
        UpdateCursorMode(true);
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        UpdateCursorMode(false);

        if (!showingCrosshair ||
            crosshairRoot == null ||
            canvas == null)
        {
            return;
        }

        RectTransform canvasRect =
            canvas.transform
                as RectTransform;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                canvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera,
                out Vector2 localPoint);

        crosshairRoot.anchoredPosition =
            localPoint + offset;
    }

    private void UpdateCursorMode(bool force)
    {
        bool uiOpen =
            IsAnyUIScreenOpen();

        bool buildCActive =
            weaponController != null &&
            weaponController
                .AutoTargetEnemies;

        bool nextShowingCrosshair =
            !uiOpen &&
            !buildCActive;

        if (!force &&
            uiOpen == usingUiCursor &&
            nextShowingCrosshair ==
            showingCrosshair)
        {
            return;
        }

        usingUiCursor = uiOpen;
        showingCrosshair =
            nextShowingCrosshair;

        if (crosshairRoot != null)
        {
            crosshairRoot.gameObject
                .SetActive(showingCrosshair);
        }

        Cursor.visible =
            uiOpen ||
            (!hideSystemCursorDuringGameplay &&
             !showingCrosshair);

        Cursor.lockState =
            CursorLockMode.None;
    }

    private bool IsAnyUIScreenOpen()
    {
        if (uiScreens == null)
            return false;

        foreach (GameObject screen
                 in uiScreens)
        {
            if (screen != null &&
                screen.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private void BuildCrosshair()
    {
        GameObject rootObject =
            new GameObject("Crosshair");

        rootObject.transform.SetParent(
            canvas.transform,
            false);

        crosshairRoot =
            rootObject.AddComponent
                <RectTransform>();

        crosshairRoot.anchorMin =
            new Vector2(0.5f, 0.5f);

        crosshairRoot.anchorMax =
            new Vector2(0.5f, 0.5f);

        crosshairRoot.pivot =
            new Vector2(0.5f, 0.5f);

        crosshairRoot.sizeDelta =
            Vector2.zero;

        CreateBar(
            "Top",
            new Vector2(
                0f,
                gap + length * 0.5f),
            new Vector2(
                thickness,
                length));

        CreateBar(
            "Bottom",
            new Vector2(
                0f,
                -gap - length * 0.5f),
            new Vector2(
                thickness,
                length));

        CreateBar(
            "Right",
            new Vector2(
                gap + length * 0.5f,
                0f),
            new Vector2(
                length,
                thickness));

        CreateBar(
            "Left",
            new Vector2(
                -gap - length * 0.5f,
                0f),
            new Vector2(
                length,
                thickness));
    }

    private void CreateBar(
        string objectName,
        Vector2 position,
        Vector2 size)
    {
        GameObject barObject =
            new GameObject(objectName);

        barObject.transform.SetParent(
            crosshairRoot,
            false);

        RectTransform rectTransform =
            barObject.AddComponent
                <RectTransform>();

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            position;

        rectTransform.sizeDelta =
            size;

        Image image =
            barObject.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = false;
    }

    private Canvas CreateCrosshairCanvas()
    {
        GameObject canvasObject =
            new GameObject(
                "CrosshairCanvas");

        Canvas newCanvas =
            canvasObject
                .AddComponent<Canvas>();

        newCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        newCanvas.sortingOrder = 5000;

        canvasObject
            .AddComponent<CanvasScaler>();

        canvasObject
            .AddComponent<GraphicRaycaster>();

        return newCanvas;
    }
}
