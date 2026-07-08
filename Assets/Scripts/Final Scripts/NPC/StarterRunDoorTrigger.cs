using UnityEngine;

public class StarterRunDoorTrigger : MonoBehaviour
{
    [SerializeField] private DoorController door;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private string playerTag = "Player";

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (door == null || !door.IsOpen) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;
        roomManager.BeginRun();
    }
}
