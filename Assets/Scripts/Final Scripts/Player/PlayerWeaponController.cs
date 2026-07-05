using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private PlayerWeaponBase startingWeapon;
    [SerializeField] private bool autoFireWhenStationary = true;
    [SerializeField] private bool allowAttackWhileMoving = false;

    private PlayerMovement movement;
    private PlayerWeaponBase currentWeapon;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        currentWeapon = startingWeapon;
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        if (movement.IsMoving && !allowAttackWhileMoving)
            return;

        if (autoFireWhenStationary || Input.GetMouseButton(0))
            currentWeapon.TryAttack(movement.AimDirection);
    }

    public void EquipWeapon(PlayerWeaponBase weapon)
    {
        currentWeapon = weapon;
    }
}
