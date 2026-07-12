using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private PlayerWeaponBase startingWeapon;
    [SerializeField] private bool autoFireWhenStationary = true;
    [SerializeField] private bool allowAttackWhileMoving;

    private PlayerMovement movement;
    private PlayerWeaponBase currentWeapon;

    public PlayerWeaponBase CurrentWeapon => currentWeapon;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        currentWeapon = startingWeapon;
    }

    private void Update()
    {
        if (currentWeapon == null)
            return;

        if (movement.IsMoving && !allowAttackWhileMoving)
            return;

        bool wantsToAttack =
            autoFireWhenStationary ||
            Input.GetMouseButton(0);

        if (wantsToAttack)
            currentWeapon.TryAttack(movement.AimDirection);
    }

    public void EquipWeapon(PlayerWeaponBase weapon)
    {
        if (weapon != null)
            currentWeapon = weapon;
    }

    public void SetAllowAttackWhileMoving(bool value)
    {
        allowAttackWhileMoving = value;
    }

    public void SetAutoFireWhenStationary(bool value)
    {
        autoFireWhenStationary = value;
    }
}
