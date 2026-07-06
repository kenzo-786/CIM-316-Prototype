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
        Debug.Log("Something entered exit trigger: " + other.name);

        if (roomManager == null)
        {
            Debug.LogWarning("RoomExitTrigger has no RoomManager.");
            return;
        }

        if (door == null)
        {
            Debug.LogWarning("RoomExitTrigger has no Door assigned.");
            return;
        }

        if (!door.IsOpen)
        {
            Debug.Log("Door is still closed.");
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            Debug.Log("Object is not tagged Player.");
            return;
        }

        Debug.Log("Player entered open door. Loading next room.");
        roomManager.LoadNextRoom();
    }
}
