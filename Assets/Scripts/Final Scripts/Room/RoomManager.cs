using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomData[] roomSequence;
    [SerializeField] private Transform activeRoomRoot;
    [SerializeField] private Transform player;

    [Header("Debug")]
    [SerializeField] private KeyCode debugClearRoomKey = KeyCode.C;

    private RoomLayout currentLayout;
    private int currentRoomIndex = -1;
    private bool roomCleared;

    private void Start()
    {
        LoadRoom(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugClearRoomKey))
            ClearCurrentRoom();
    }

    public void LoadNextRoom()
    {
        LoadRoom(currentRoomIndex + 1);
    }

    public void LoadRoom(int index)
    {
        if (index >= roomSequence.Length)
        {
            Debug.Log("Run complete. Later this opens the win screen.");
            return;
        }

        currentRoomIndex = index;
        roomCleared = false;

        if (currentLayout != null)
            Destroy(currentLayout.gameObject);

        RoomData roomData = roomSequence[currentRoomIndex];

        currentLayout = Instantiate(
            roomData.layoutPrefab,
            activeRoomRoot.position,
            Quaternion.identity,
            activeRoomRoot
        );

        currentLayout.PrepareRoom();

        RoomExitTrigger exitTrigger = currentLayout.GetComponentInChildren<RoomExitTrigger>();
        if (exitTrigger != null)
            exitTrigger.Initialize(this);

        if (player != null && currentLayout.PlayerSpawnPoint != null)
            player.position = currentLayout.PlayerSpawnPoint.position;
    }

    public void ClearCurrentRoom()
    {
        if (roomCleared) return;

        roomCleared = true;
        currentLayout.OpenExit();
    }
}
