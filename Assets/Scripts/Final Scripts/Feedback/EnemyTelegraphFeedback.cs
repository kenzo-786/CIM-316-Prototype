using UnityEngine;

public class EnemyTelegraphFeedback : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private Color warningColor = new Color(1f, 0.25f, 0.1f, 1f);
    [SerializeField, Min(0f)] private float pulseSpeed = 12f;

    [Header("Start Cue")]
    [SerializeField] private Transform effectOrigin;
    [SerializeField] private GameObject telegraphEffectPrefab;
    [SerializeField] private string telegraphSoundId;

    [Header("Optional World Marker")]
    [SerializeField] private GameObject worldMarkerPrefab;

    private Color[] normalColors;
    private GameObject worldMarker;
    private float remainingTime;
    private bool active;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        normalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                normalColors[i] = renderers[i].color;
            }
        }

        if (worldMarkerPrefab != null)
        {
            worldMarker = Instantiate(worldMarkerPrefab);
            worldMarker.SetActive(false);
        }
    }

    private void Update()
    {
        if (!active)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = Color.Lerp(normalColors[i], warningColor, pulse);
            }
        }

        if (remainingTime <= 0f)
        {
            End();
        }
    }

    public void Begin(float duration)
    {
        active = true;
        remainingTime = Mathf.Max(0.01f, duration);

        Vector3 position = effectOrigin != null
            ? effectOrigin.position
            : transform.position;

        FeedbackEventBus.SpawnEffect(telegraphEffectPrefab, position);
        FeedbackEventBus.PlaySound(telegraphSoundId, position);

        if (worldMarker != null)
        {
            worldMarker.SetActive(false);
        }
    }

    public void BeginAtPosition(float duration, Vector2 position, float diameter)
    {
        Begin(duration);

        if (worldMarker == null)
        {
            return;
        }

        worldMarker.transform.position = position;
        worldMarker.transform.localScale = new Vector3(diameter, diameter, 1f);
        worldMarker.SetActive(true);
    }

    public void MoveMarker(Vector2 position)
    {
        if (worldMarker != null && worldMarker.activeSelf)
        {
            worldMarker.transform.position = position;
        }
    }

    public void End()
    {
        active = false;
        remainingTime = 0f;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = normalColors[i];
            }
        }

        if (worldMarker != null)
        {
            worldMarker.SetActive(false);
        }
    }

    private void OnDisable()
    {
        End();
    }

    private void OnDestroy()
    {
        if (worldMarker != null)
        {
            Destroy(worldMarker);
        }
    }
}
