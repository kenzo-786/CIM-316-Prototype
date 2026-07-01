using UnityEngine;

public class RoomLayout : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnRoot;
    [SerializeField] private DoorController exitDoor;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform EnemySpawnRoot => enemySpawnRoot;
    public DoorController ExitDoor => exitDoor;

    public void PrepareRoom()
    {
        exitDoor.CloseAndLock();
    }

    public void OpenExit()
    {
        exitDoor.OpenAndUnlock();
    }
}
