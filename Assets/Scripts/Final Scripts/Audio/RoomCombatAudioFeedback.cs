using UnityEngine;

public class RoomCombatAudioFeedback : MonoBehaviour
{
    [SerializeField] private RoomCombatController combatController;
    [SerializeField] private string waveWarningSoundId = "wave_warning";
    [SerializeField] private string roomClearSoundId = "room_clear";
    [SerializeField] private string xpMagnetSoundId = "xp_magnet";

    private void Awake()
    {
        if (combatController == null)
            combatController = GetComponent<RoomCombatController>();
    }

    private void OnEnable()
    {
        if (combatController == null)
            return;

        combatController.OnWaveWarning += HandleWaveWarning;
        combatController.OnRoomCombatCleared += HandleRoomCleared;
    }

    private void OnDisable()
    {
        if (combatController == null)
            return;

        combatController.OnWaveWarning -= HandleWaveWarning;
        combatController.OnRoomCombatCleared -= HandleRoomCleared;
    }

    private void HandleWaveWarning(int waveNumber, int totalWaves, float duration)
    {
        FeedbackEventBus.PlaySound(waveWarningSoundId, transform.position);
    }

    private void HandleRoomCleared()
    {
        FeedbackEventBus.PlaySound(roomClearSoundId, transform.position);
        FeedbackEventBus.PlaySound(xpMagnetSoundId, transform.position);
    }
}
