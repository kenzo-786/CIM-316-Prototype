using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Run/Room Run Config")]
public class RoomRunConfig : ScriptableObject
{
    [Header("Room Pools")]
    [SerializeField] private RoomData[] normalRooms;
    [SerializeField] private RoomData[] specialRooms;
    [SerializeField] private RoomData[] bossRooms;
    [SerializeField] private RoomData finalRoom;

    [Header("Rules")]
    [SerializeField] private int specialRoomEvery = 5;
    [SerializeField] private int bossRoomEvery = 10;

    public RoomData GetRoomData(int roomIndex, int totalRooms)
    {
        int roomNumber = roomIndex + 1;

        if (roomNumber == totalRooms && finalRoom != null)
            return finalRoom;

        if (bossRoomEvery > 0 && roomNumber % bossRoomEvery == 0)
            return PickRandom(bossRooms, normalRooms);

        if (specialRoomEvery > 0 && roomNumber % specialRoomEvery == 0)
            return PickRandom(specialRooms, normalRooms);

        return PickRandom(normalRooms, null);
    }

    private RoomData PickRandom(RoomData[] primary, RoomData[] fallback)
    {
        if (primary != null && primary.Length > 0)
            return primary[Random.Range(0, primary.Length)];

        if (fallback != null && fallback.Length > 0)
            return fallback[Random.Range(0, fallback.Length)];

        return null;
    }
}
