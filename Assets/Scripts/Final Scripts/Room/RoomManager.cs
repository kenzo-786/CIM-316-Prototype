using System;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private RoomData[] roomSequence;
    [SerializeField] private RoomRunConfig runConfig;
    [SerializeField] private int totalRooms = 30;
    [SerializeField] private bool autoLoadFirstRoom = true;

    [Header("Scene References")]
    [SerializeField] private Transform activeRoomRoot;
    [SerializeField] private Transform player;
    [SerializeField] private RoomCombatController combatController;

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

    public void BeginRun()
    {
        if (currentRoomIndex >= 0 || runEnded)
            return;

        LoadRoom(0);
    }

    public void LoadNextRoom()
    {
        LoadRoom(currentRoomIndex + 1);
    }

    public void LoadRoom(int index)
    {
        if (runEnded)
            return;

        if (index >= totalRooms)
        {
            EndRunAsWin();
            return;
        }

        RoomData roomData = GetRoomData(index);

        if (roomData == null || roomData.layoutPrefab == null)
        {
            Debug.LogError("RoomManager cannot load room. RoomData or layout prefab is missing.", this);
            return;
        }

        currentRoomIndex = index;
        roomCleared = false;

        if (currentLayout != null)
            Destroy(currentLayout.gameObject);

        Transform parent = activeRoomRoot != null ? activeRoomRoot : transform;

        currentLayout = Instantiate(
            roomData.layoutPrefab,
            parent.position,
            Quaternion.identity,
            parent
        );

        currentLayout.PrepareRoom();

        RoomExitTrigger exitTrigger = currentLayout.GetComponentInChildren<RoomExitTrigger>();
        if (exitTrigger != null)
            exitTrigger.Initialize(this);

        if (player != null && currentLayout.PlayerSpawnPoint != null)
            player.position = currentLayout.PlayerSpawnPoint.position;

        if (combatController != null)
        {
            combatController.OnRoomCombatCleared -= ClearCurrentRoom;
            combatController.OnRoomCombatCleared += ClearCurrentRoom;
            combatController.StartRoomCombat(roomData, currentLayout, player, currentRoomIndex);
        }
        else
        {
            Debug.LogWarning("RoomManager has no RoomCombatController assigned.", this);
        }

        OnRoomChanged?.Invoke(CurrentRoomNumber, totalRooms);
    }

    public void ClearCurrentRoom()
    {
        if (roomCleared || currentLayout == null)
            return;

        roomCleared = true;
        currentLayout.OpenExit();
    }

    private RoomData GetRoomData(int index)
    {
        if (runConfig != null)
            return runConfig.GetRoomData(index, totalRooms);

        if (roomSequence == null || roomSequence.Length == 0)
        {
            Debug.LogError("No rooms assigned to RoomManager.", this);
            return null;
        }

        return roomSequence[index % roomSequence.Length];
    }

    private void EndRunAsWin()
    {
        runEnded = true;
        Time.timeScale = 0f;
        OnRunWon?.Invoke();
        Debug.Log("Run won.", this);
    }
}
