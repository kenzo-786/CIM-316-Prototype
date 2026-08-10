using UnityEngine;

public class TutorialExitTrigger : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private DoorController door;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (door == null || !door.IsOpen)
        {
            return;
        }

        if (tutorialManager != null)
        {
            tutorialManager.CompleteTutorialAndLoadGame();
        }
    }
}
