using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerWeaponController))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Direction")]
    [SerializeField] private Vector2 startingFacingDirection = Vector2.down;
    [SerializeField, Min(0f)] private float movementThreshold = 0.01f;

    [Header("Attack")]
    [SerializeField] private bool playAttackAnimations;

    private static readonly int MoveXHash =
        Animator.StringToHash("MoveX");

    private static readonly int MoveYHash =
        Animator.StringToHash("MoveY");

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int AimXHash =
        Animator.StringToHash("AimX");

    private static readonly int AimYHash =
        Animator.StringToHash("AimY");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private PlayerMovement movement;
    private PlayerWeaponController weaponController;
    private Vector2 lastMovementDirection;
    private Vector2 lastAimDirection;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        weaponController = GetComponent<PlayerWeaponController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        lastMovementDirection =
            SnapToCardinal(startingFacingDirection);

        lastAimDirection = lastMovementDirection;
    }

    private void OnEnable()
    {
        if (weaponController != null)
        {
            weaponController.OnAttackPerformed += HandleAttack;
        }
    }

    private void OnDisable()
    {
        if (weaponController != null)
        {
            weaponController.OnAttackPerformed -= HandleAttack;
        }
    }

    private void LateUpdate()
    {
        if (animator == null || movement == null)
        {
            return;
        }

        Vector2 moveInput = movement.MoveInput;

        bool isMoving =
            moveInput.sqrMagnitude >
            movementThreshold * movementThreshold;

        if (isMoving)
        {
            lastMovementDirection =
                SnapToCardinal(moveInput);
        }

        animator.SetFloat(
            MoveXHash,
            lastMovementDirection.x
        );

        animator.SetFloat(
            MoveYHash,
            lastMovementDirection.y
        );

        animator.SetFloat(
            SpeedHash,
            isMoving ? 1f : 0f
        );

        animator.SetFloat(
            AimXHash,
            lastAimDirection.x
        );

        animator.SetFloat(
            AimYHash,
            lastAimDirection.y
        );
    }

    private void HandleAttack(Vector2 attackDirection)
    {
        if (animator == null)
        {
            return;
        }

        if (attackDirection.sqrMagnitude > 0.01f)
        {
            lastAimDirection =
                SnapToCardinal(attackDirection);
        }

        animator.SetFloat(
            AimXHash,
            lastAimDirection.x
        );

        animator.SetFloat(
            AimYHash,
            lastAimDirection.y
        );

        if (!playAttackAnimations)
        {
            return;
        }

        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);
    }

    public void SetAttackAnimationsEnabled(bool enabled)
    {
        playAttackAnimations = enabled;
    }

    private static Vector2 SnapToCardinal(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            return Vector2.down;
        }

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x >= 0f
                ? Vector2.right
                : Vector2.left;
        }

        return direction.y >= 0f
            ? Vector2.up
            : Vector2.down;
    }
}
