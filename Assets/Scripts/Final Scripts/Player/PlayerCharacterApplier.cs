using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerWeaponController))]
public class PlayerCharacterApplier : MonoBehaviour
{
    [SerializeField] private PlayerCharacterData fallbackCharacter;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer playerVisual;

    [Header("Weapons")]
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private RulerSlashWeapon rulerWeapon;
    [SerializeField] private EraserThrowWeapon eraserWeapon;

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
            Debug.LogError(
                "PlayerCharacterApplier has no selected or fallback character.",
                this);

            return;
        }

        PlayerMovement movement =
            GetComponent<PlayerMovement>();

        Health health =
            GetComponent<Health>();

        if (weaponController == null)
            weaponController =
                GetComponent<PlayerWeaponController>();

        movement.SetMoveSpeed(data.moveSpeed);
        health.SetMaxHealth(data.maxHealth, true);

        if (playerVisual != null &&
            data.gameplaySprite != null)
        {
            playerVisual.sprite = data.gameplaySprite;
        }

        if (weaponController != null)
        {
            weaponController.SetAllowAttackWhileMoving(
                data.canAttackWhileMoving);

            switch (data.weaponType)
            {
                case PlayerWeaponType.Ruler:
                    weaponController.EquipWeapon(rulerWeapon);
                    break;

                case PlayerWeaponType.Eraser:
                    weaponController.EquipWeapon(eraserWeapon);
                    break;
            }
        }

        Debug.Log(
            "Applied selected character: " +
            data.characterName,
            this);
    }
}
