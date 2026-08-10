using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossWebTrap : MonoBehaviour
{
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float defaultDuration = 5f;
    [SerializeField] private float defaultSpeedMultiplier = 0.7f;

    private readonly HashSet<PlayerMovement>
        affectedPlayers =
            new HashSet<PlayerMovement>();

    private float remainingDuration;
    private float speedMultiplier;

    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        triggerCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        remainingDuration = defaultDuration;
        speedMultiplier = defaultSpeedMultiplier;
    }

    private void Update()
    {
        remainingDuration -= Time.deltaTime;

        if (remainingDuration <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        RemoveAllSlows();
    }

    public void Initialize(
        float duration,
        float movementMultiplier)
    {
        remainingDuration =
            Mathf.Max(0.1f, duration);

        speedMultiplier =
            Mathf.Clamp(movementMultiplier, 0.05f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsInPlayerLayer(other.gameObject.layer))
        {
            return;
        }

        PlayerMovement movement =
            other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        affectedPlayers.Add(movement);
        movement.SetSpeedModifier(this, speedMultiplier);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement movement =
            other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        movement.RemoveSpeedModifier(this);
        affectedPlayers.Remove(movement);
    }

    private void RemoveAllSlows()
    {
        foreach (PlayerMovement movement in affectedPlayers)
        {
            if (movement != null)
            {
                movement.RemoveSpeedModifier(this);
            }
        }

        affectedPlayers.Clear();
    }

    private bool IsInPlayerLayer(int layer)
    {
        return
            (playerLayer.value & (1 << layer)) != 0;
    }
}
