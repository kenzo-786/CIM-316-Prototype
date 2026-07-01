using UnityEditor.EditorTools;
using UnityEngine;

public class RoomExitTrigger : MonoBehaviour
{
    [SerializeField] private DoorController door;
    [SerializeField] private string playerTag = "Player";

    private RoomManager roomManager;

    public void Initialize(RoomManager manager)
    {
        roomManager = manager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (roomManager == null) return;
        if (door == null || !door.IsOpen) return;
        if (!other.CompareTag(playerTag)) return;

        roomManager.LoadNextRoom();
    }
}
