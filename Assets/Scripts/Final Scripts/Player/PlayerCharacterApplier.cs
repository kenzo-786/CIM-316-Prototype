using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerWeaponController))]
public class PlayerCharacterApplier : MonoBehaviour
{
    [SerializeField] private PlayerCharacterData fallbackCharacter;
    [SerializeField] private SpriteRenderer playerVisual;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private RulerSlashWeapon rulerWeapon;
    [SerializeField] private EraserThrowWeapon eraserWeapon;

    private void Awake()
    {
        if (playerVisual == null)
            playerVisual = GetComponentInChildren<SpriteRenderer>();

        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();
    }

    private void Start()
    {
        ApplySelectedCharacter();
    }

    public void ApplySelectedCharacter()
    {
        PlayerCharacterData data =
            SelectedCharacter.CharacterData != null
                ? SelectedCharacter.CharacterData
                : fallbackCharacter;

        if (data == null)
        {
            Debug.LogError("PlayerCharacterApplier has no selected or fallback character.", this);
            return;
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        Health health = GetComponent<Health>();

        movement.SetMoveSpeed(data.moveSpeed);
        health.SetMaxHealth(data.maxHealth, true);

        if (playerVisual != null && data.gameplaySprite != null)
            playerVisual.sprite = data.gameplaySprite;

        if (playerAnimator != null && data.animatorController != null)
        {
            playerAnimator.runtimeAnimatorController = data.animatorController;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        if (weaponController == null)
            return;

        ApplyFiringMode(data.firingMode);

        switch (data.weaponType)
        {
            case PlayerWeaponType.Ruler:
                weaponController.EquipWeapon(rulerWeapon);
                break;

            case PlayerWeaponType.Eraser:
                weaponController.EquipWeapon(eraserWeapon);
                break;
        }

        MetaProgressionManager.Instance?.ApplyToPlayer(gameObject);
    }

    private void ApplyFiringMode(PlayerFiringMode firingMode)
    {
        bool canMoveAndShoot =
            firingMode == PlayerFiringMode.BuildBMoveAndShootMouseAim;

        bool usesAutoTarget =
            firingMode == PlayerFiringMode.BuildCStationaryAutoTarget;

        weaponController.SetAllowAttackWhileMoving(canMoveAndShoot);
        weaponController.SetAutoTargeting(usesAutoTarget);
        weaponController.SetAutoFireWhenStationary(true);
    }
}
