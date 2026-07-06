using UnityEngine;

public class RoomLayout : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnRoot;
    [SerializeField] private DoorController exitDoor;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform EnemySpawnRoot => enemySpawnRoot;
    public DoorController ExitDoor => exitDoor;

    private void Awake()
    {
        if (exitDoor == null)
        {
            exitDoor = GetComponentInChildren<DoorController>();
        }

        if (exitDoor == null)
        {
            Debug.LogError("RoomLayout could not find Exit Door.", this);
        }
    }

    public void PrepareRoom()
    {
        if (exitDoor == null)
        {
            Debug.LogError("PrepareRoom failed: Exit Door is missing.", this);
            return;
        }

        exitDoor.CloseAndLock();
    }

    public void OpenExit()
    {
        if (exitDoor == null)
        {
            Debug.LogError("OpenExit failed: Exit Door is missing.", this);
            return;
        }

        Debug.Log("RoomLayout opening exit door.");
        exitDoor.OpenAndUnlock();
    }
}
