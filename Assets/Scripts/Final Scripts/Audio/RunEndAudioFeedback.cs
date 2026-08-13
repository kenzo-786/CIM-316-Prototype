using System.Collections;
using UnityEngine;

public class RunEndAudioFeedback : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Health playerHealth;
    [SerializeField] private string lossSoundId = "game_over";
    [SerializeField] private string victorySoundId = "run_victory";

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindObjectOfType<RoomManager>();

        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerMovement>()?.GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (roomManager != null)
            roomManager.OnRunWon += PlayVictory;

        if (playerHealth != null)
            playerHealth.OnDied += CheckFinalDeath;
    }

    private void OnDisable()
    {
        if (roomManager != null)
            roomManager.OnRunWon -= PlayVictory;

        if (playerHealth != null)
            playerHealth.OnDied -= CheckFinalDeath;
    }

    private void CheckFinalDeath()
    {
        StartCoroutine(PlayLossAfterReviveCheck());
    }

    private IEnumerator PlayLossAfterReviveCheck()
    {
        yield return null;

        if (playerHealth != null && playerHealth.IsDead)
            FeedbackEventBus.PlaySound(lossSoundId, transform.position);
    }

    private void PlayVictory()
    {
        FeedbackEventBus.PlaySound(victorySoundId, transform.position);
    }
}
