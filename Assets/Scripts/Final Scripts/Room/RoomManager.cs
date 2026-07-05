using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomData[] roomSequence;
    [SerializeField] private int totalRooms = 30;
    [SerializeField] private Transform activeRoomRoot;
    [SerializeField] private Transform player;
    [SerializeField] private RoomCombatController combatController;

    private RoomLayout currentLayout;
    private int currentRoomIndex = -1;
    private bool roomCleared;

    private void Start()
    {
        LoadRoom(0);
    }

    public void LoadNextRoom()
    {
        LoadRoom(currentRoomIndex + 1);
    }

    public void LoadRoom(int index)
    {
        if (index >= totalRooms)
        {
            Debug.Log("Run complete. Later this opens the win screen.");
            return;
        }

        currentRoomIndex = index;
        roomCleared = false;

        if (currentLayout != null)
            Destroy(currentLayout.gameObject);

        RoomData roomData = GetRoomData(index);

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

        combatController.OnRoomCombatCleared -= ClearCurrentRoom;
        combatController.OnRoomCombatCleared += ClearCurrentRoom;
        combatController.StartRoomCombat(roomData, currentLayout, player);
    }

    public void ClearCurrentRoom()
    {
        if (roomCleared) return;

        roomCleared = true;
        currentLayout.OpenExit();
    }

    private RoomData GetRoomData(int index)
    {
        if (roomSequence.Length == 0)
        {
            Debug.LogError("No rooms assigned to RoomManager.");
            return null;
        }

        return roomSequence[index % roomSequence.Length];
    }
}
