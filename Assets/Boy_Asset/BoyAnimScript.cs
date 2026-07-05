using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;

    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Remember the last movement direction
        if (movement.IsMoving)
        {
            lastDirection = movement.MoveInput;
        }

        // Walking Bool
        animator.SetBool("IsWalking", movement.IsMoving);

        // Blend Tree Parameters
        animator.SetFloat("XInput", lastDirection.x);
        animator.SetFloat("YInput", lastDirection.y);
    }
}