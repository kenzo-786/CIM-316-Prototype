using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Color closedColor = new Color(0.45f, 0.12f, 0.12f);
    [SerializeField] private Color openColor = new Color(0.15f, 0.55f, 0.25f);

    public bool IsOpen { get; private set; }

    public void CloseAndLock()
    {
        IsOpen = false;

        solidCollider.enabled = true;
        triggerCollider.enabled = false;

        if (visual != null)
            visual.color = closedColor;
    }

    public void OpenAndUnlock()
    {
        IsOpen = true;

        solidCollider.enabled = false;
        triggerCollider.enabled = true;

        if (visual != null)
            visual.color = openColor;
    }
}
