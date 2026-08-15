using UnityEngine;

public class DoorVisualFeedback : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField, Min(0f)] private float closedPulseSpeed = 1.8f;
    [SerializeField, Min(0f)] private float openPulseSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float closedBrightness = 0.14f;
    [SerializeField, Range(0f, 1f)] private float openBrightness = 0.6f;
    [SerializeField, Range(0f, 1f)] private float closedGlowAlpha = 0.14f;
    [SerializeField, Range(0f, 1f)] private float openGlowAlpha = 0.68f;

    private SpriteRenderer glow;
    private Color stateColor = Color.white;
    private bool isOpen;
    private bool initialized;

    public void Initialize(SpriteRenderer targetVisual)
    {
        visual = targetVisual != null
            ? targetVisual
            : GetComponent<SpriteRenderer>();

        if (visual == null)
            return;

        EnsureGlow();
        initialized = true;
    }

    public void SetState(bool open, Color color)
    {
        if (!initialized)
            Initialize(visual);

        isOpen = open;
        stateColor = color;

        if (visual != null)
            visual.color = stateColor;

        SyncGlow();
    }

    private void Update()
    {
        if (!initialized || visual == null)
            return;

        float speed = isOpen ? openPulseSpeed : closedPulseSpeed;
        float wave = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;

        if (isOpen)
            wave = Mathf.SmoothStep(0f, 1f, wave);
        float brightness = isOpen ? openBrightness : closedBrightness;
        float glowAlpha = isOpen ? openGlowAlpha : closedGlowAlpha;

        visual.color = Color.Lerp(stateColor, Color.white, brightness * wave);

        if (glow == null)
            return;

        SyncGlowSprite();
        Color glowColor = stateColor;
        glowColor.a = glowAlpha * Mathf.Lerp(0.45f, 1f, wave);
        glow.color = glowColor;
        float minimumScale = isOpen ? 1.07f : 1.025f;
        float maximumScale = isOpen ? 1.2f : 1.06f;
        glow.transform.localScale = Vector3.one * Mathf.Lerp(minimumScale, maximumScale, wave);
    }

    private void EnsureGlow()
    {
        Transform existing = visual.transform.Find("Door Glow");

        if (existing != null)
            glow = existing.GetComponent<SpriteRenderer>();

        if (glow == null)
        {
            GameObject glowObject = new GameObject("Door Glow");
            glowObject.layer = visual.gameObject.layer;
            glowObject.transform.SetParent(visual.transform, false);
            glow = glowObject.AddComponent<SpriteRenderer>();
        }

        SyncGlowSprite();
        glow.sortingLayerID = visual.sortingLayerID;
        glow.sortingOrder = visual.sortingOrder + 1;
        glow.enabled = visual.enabled;
    }

    private void SyncGlow()
    {
        if (glow == null)
            return;

        SyncGlowSprite();
        Color glowColor = stateColor;
        glowColor.a = isOpen ? openGlowAlpha : closedGlowAlpha;
        glow.color = glowColor;
    }

    private void SyncGlowSprite()
    {
        if (glow == null || visual == null)
            return;

        glow.sprite = visual.sprite;
        glow.sharedMaterial = visual.sharedMaterial;
        glow.flipX = visual.flipX;
        glow.flipY = visual.flipY;
        glow.drawMode = visual.drawMode;
        glow.size = visual.size;
    }

    private void OnDisable()
    {
        if (visual != null)
        {
            visual.color = stateColor;
        }
    }
}
