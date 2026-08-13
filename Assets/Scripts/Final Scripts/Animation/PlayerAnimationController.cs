using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerWeaponController))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private Transform weaponPivot;

    [Header("Facing")]
    [SerializeField] private Vector2 startingDirection = Vector2.down;
    [SerializeField] private bool faceAimWhenStationary = true;
    [SerializeField] private float weaponTurnSpeed = 720f;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int XInputHash = Animator.StringToHash("XInput");
    private static readonly int YInputHash = Animator.StringToHash("YInput");

    private Vector2 lastDirection;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();

        lastDirection = SnapToCardinal(startingDirection);
    }

    private void OnEnable()
    {
        if (weaponController != null)
            weaponController.OnAttackPerformed += HandleAttackPerformed;
    }

    private void OnDisable()
    {
        if (weaponController != null)
            weaponController.OnAttackPerformed -= HandleAttackPerformed;
    }

    private void LateUpdate()
    {
        if (animator == null || movement == null)
            return;

        Vector2 aimDirection = GetAimDirection();

        if (movement.IsMoving)
            lastDirection = SnapToCardinal(movement.MoveInput);
        else if (faceAimWhenStationary && aimDirection.sqrMagnitude > 0.01f)
            lastDirection = SnapToCardinal(aimDirection);

        animator.SetBool(IsWalkingHash, movement.IsMoving);
        animator.SetFloat(XInputHash, lastDirection.x);
        animator.SetFloat(YInputHash, lastDirection.y);

        UpdateWeaponPivot(aimDirection);
    }

    private void HandleAttackPerformed(Vector2 attackDirection)
    {
        if (animator == null)
            return;

        if (attackDirection.sqrMagnitude > 0.01f)
            lastDirection = SnapToCardinal(attackDirection);

        animator.SetFloat(XInputHash, lastDirection.x);
        animator.SetFloat(YInputHash, lastDirection.y);
        animator.SetTrigger(IsAttackingHash);
    }

    private Vector2 GetAimDirection()
    {
        if (weaponController != null &&
            weaponController.AutoTargetEnemies &&
            weaponController.CurrentTarget != null)
        {
            return ((Vector2)weaponController.CurrentTarget.position -
                    (Vector2)transform.position).normalized;
        }

        return movement.AimDirection.normalized;
    }

    private void UpdateWeaponPivot(Vector2 aimDirection)
    {
        if (weaponPivot == null || aimDirection.sqrMagnitude <= 0.01f)
            return;

        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float currentAngle = weaponPivot.localEulerAngles.z;
        float smoothAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            weaponTurnSpeed * Time.deltaTime
        );

        weaponPivot.localRotation = Quaternion.Euler(0f, 0f, smoothAngle);
    }

    private static Vector2 SnapToCardinal(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
            return Vector2.down;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x >= 0f ? Vector2.right : Vector2.left;

        return direction.y >= 0f ? Vector2.up : Vector2.down;
    }
}
