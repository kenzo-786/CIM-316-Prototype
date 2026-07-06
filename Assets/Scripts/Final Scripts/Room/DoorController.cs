using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private SpriteRenderer visual;

    [SerializeField] private Color closedColor = Color.red;
    [SerializeField] private Color openColor = Color.green;

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
        else
            Debug.LogError("Door missing Solid Collider.", this);

        if (triggerCollider != null)
            triggerCollider.enabled = false;
        else
            Debug.LogError("Door missing Trigger Collider.", this);

        if (visual != null)
            visual.color = closedColor;

        Debug.Log("Door closed and locked.", this);
    }

    public void OpenAndUnlock()
    {
        IsOpen = true;

        if (solidCollider != null)
            solidCollider.enabled = false;
        else
            Debug.LogError("Door missing Solid Collider.", this);

        if (triggerCollider != null)
            triggerCollider.enabled = true;
        else
            Debug.LogError("Door missing Trigger Collider.", this);

        if (visual != null)
            visual.color = openColor;
        else
            Debug.LogError("Door missing Visual SpriteRenderer.", this);

        Debug.Log("Door opened and unlocked.", this);
    }
}
