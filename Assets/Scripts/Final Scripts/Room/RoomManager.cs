using System;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomData[] roomSequence;
    [SerializeField] private int totalRooms = 5;
    [SerializeField] private Transform activeRoomRoot;
    [SerializeField] private Transform player;
    [SerializeField] private RoomCombatController combatController;
    [SerializeField] private bool autoLoadFirstRoom = true;

    public event Action<int, int> OnRoomChanged;
    public event Action OnRunWon;

    private RoomLayout currentLayout;
    private int currentRoomIndex = -1;
    private bool roomCleared;
    private bool runEnded;

    public int CurrentRoomNumber => currentRoomIndex + 1;
    public int TotalRooms => totalRooms;

    private void Start()
    {
        Time.timeScale = 1f;

        if (autoLoadFirstRoom)
            LoadRoom(0);
    }

    public void LoadNextRoom()
    {
        LoadRoom(currentRoomIndex + 1);
    }

    public void BeginRun()
    {
        if (currentRoomIndex >= 0) return;
        LoadRoom(0);
    }

    public void LoadRoom(int index)
    {
        if (runEnded) return;

        if (index >= totalRooms)
        {
            runEnded = true;
            Time.timeScale = 0f;
            OnRunWon?.Invoke();
            Debug.Log("Run won.");
            return;
        }

        currentRoomIndex = index;
        roomCleared = false;

        if (currentLayout != null)
            Destroy(currentLayout.gameObject);

        RoomData roomData = GetRoomData(index);
        if (roomData == null || roomData.layoutPrefab == null)
        {
            Debug.LogError("Room data or layout prefab missing.");
            return;
        }

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

        OnRoomChanged?.Invoke(CurrentRoomNumber, totalRooms);
    }

    public void ClearCurrentRoom()
    {
        if (roomCleared) return;

        roomCleared = true;
        currentLayout.OpenExit();
    }

    private RoomData GetRoomData(int index)
    {
        if (roomSequence == null || roomSequence.Length == 0)
            return null;

        return roomSequence[index % roomSequence.Length];
    }
}
