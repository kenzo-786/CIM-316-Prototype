using UnityEngine;

public class RoomClearFeedback : MonoBehaviour
{
    [SerializeField] private RoomCombatController combatController;
    [SerializeField] private Transform effectOrigin;
    [SerializeField] private GameObject roomClearEffectPrefab;
    [SerializeField] private string roomClearSoundId = "room_clear";
    [SerializeField, Min(0f)] private float shakeIntensity = 0.1f;
    [SerializeField, Min(0f)] private float shakeDuration = 0.08f;

    private void OnEnable()
    {
        if (combatController != null)
        {
            combatController.OnRoomCombatCleared += Play;
        }
    }

    private void OnDisable()
    {
        if (combatController != null)
        {
            combatController.OnRoomCombatCleared -= Play;
        }
    }

    private void Play()
    {
        Vector3 position = effectOrigin != null
            ? effectOrigin.position
            : transform.position;

        FeedbackEventBus.SpawnEffect(roomClearEffectPrefab, position);
        FeedbackEventBus.PlaySound(roomClearSoundId, position);
        FeedbackEventBus.RequestScreenShake(shakeIntensity, shakeDuration, position);
    }
}
