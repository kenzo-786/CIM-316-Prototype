using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Health))]
public class PlayerCharacterApplier : MonoBehaviour
{
    [SerializeField] private PlayerCharacterData fallbackCharacter;

    private void Awake()
    {
        PlayerCharacterData data = SelectedCharacter.CharacterData != null
            ? SelectedCharacter.CharacterData
            : fallbackCharacter;

        if (data == null) return;

        GetComponent<PlayerMovement>().SetMoveSpeed(data.moveSpeed);
        GetComponent<Health>().SetMaxHealth(data.maxHealth, true);
    }
}
