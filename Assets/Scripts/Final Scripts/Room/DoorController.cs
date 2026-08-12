using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Color closedColor = Color.red;
    [SerializeField] private Color openColor = Color.green;
    [SerializeField] private string openSoundId = "door_open";

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (visual == null)
            visual = GetComponent<SpriteRenderer>();
    }

    public void CloseAndLock()
    {
        IsOpen = false;

        if (solidCollider != null)
            solidCollider.enabled = true;

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        if (visual != null)
            visual.color = closedColor;
    }

    public void OpenAndUnlock()
    {
        if (IsOpen)
            return;

        IsOpen = true;

        if (solidCollider != null)
            solidCollider.enabled = false;

        if (triggerCollider != null)
            triggerCollider.enabled = true;

        if (visual != null)
            visual.color = openColor;

        FeedbackEventBus.PlaySound(openSoundId, transform.position);
    }
}
