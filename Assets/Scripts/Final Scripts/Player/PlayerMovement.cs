using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private readonly Dictionary<Object, float> speedModifiers =
        new Dictionary<Object, float>();

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public float MoveSpeed => moveSpeed;
    public float EffectiveMoveSpeed =>
        moveSpeed * GetMovementMultiplier();

    public Vector2 MoveInput => moveInput;
    public bool IsMoving { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        IsMoving = moveInput.sqrMagnitude > 0.01f;

        UpdateMouseAim();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity =
            moveInput * EffectiveMoveSpeed;
    }

    private void OnDisable()
    {
        speedModifiers.Clear();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0f, value);
    }

    public void SetSpeedModifier(
        Object source,
        float multiplier)
    {
        if (source == null)
        {
            return;
        }

        speedModifiers[source] =
            Mathf.Clamp(multiplier, 0.05f, 10f);
    }

    public void RemoveSpeedModifier(Object source)
    {
        if (source != null)
        {
            speedModifiers.Remove(source);
        }
    }

    private float GetMovementMultiplier()
    {
        float lowestMultiplier = 1f;

        foreach (float multiplier in speedModifiers.Values)
        {
            lowestMultiplier = Mathf.Min(
                lowestMultiplier,
                multiplier
            );
        }

        return lowestMultiplier;
    }

    private void UpdateMouseAim()
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector2 direction =
            mouseWorld - transform.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            AimDirection = direction.normalized;
        }
    }
}
