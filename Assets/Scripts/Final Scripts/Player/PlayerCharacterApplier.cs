using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Health))]
public class PlayerCharacterApplier : MonoBehaviour
{
    [SerializeField] private PlayerCharacterData characterData;

    private void Awake()
    {
        if (characterData == null)
        {
            Debug.LogWarning("No character data assigned.");
            return;
        }

        GetComponent<PlayerMovement>().SetMoveSpeed(characterData.moveSpeed);
        GetComponent<Health>().SetMaxHealth(characterData.maxHealth, true);
    }
}
