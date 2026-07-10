using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Run/Room Run Config")]
public class RoomRunConfig : ScriptableObject
{
    [Header("Run Length")]
    [SerializeField] private int totalRooms = 30;

    [Header("Room Pools")]
    [SerializeField] private RoomData[] normalRooms;
    [SerializeField] private RoomData[] classroomRooms;
    [SerializeField] private RoomData[] hallwayRooms;
    [SerializeField] private RoomData[] openAreaRooms;
    [SerializeField] private RoomData[] eliteRooms;
    [SerializeField] private RoomData[] treasureRooms;
    [SerializeField] private RoomData[] restRooms;
    [SerializeField] private RoomData[] bossRooms;
    [SerializeField] private RoomData[] finalRooms;

    [Header("Special Room Rules")]
    [SerializeField] private int eliteEveryRooms = 7;
    [SerializeField] private int treasureEveryRooms = 10;
    [SerializeField] private int restEveryRooms = 12;
    [SerializeField] private int bossEveryRooms = 15;

    public int TotalRooms => Mathf.Max(1, totalRooms);

    public RoomData GetRoom(int roomIndex)
    {
        return GetRoomData(roomIndex);
    }

    public RoomData GetRoom(int roomIndex, int runTotalRooms)
    {
        return GetRoomData(roomIndex, runTotalRooms);
    }

    public RoomData GetRoomData(int roomIndex)
    {
        RoomType roomType = GetRoomType(roomIndex);
        return GetRoomFromType(roomType, roomIndex);
    }

    public RoomData GetRoomData(int roomIndex, int runTotalRooms)
    {
        RoomType roomType = GetRoomType(roomIndex, runTotalRooms);
        return GetRoomFromType(roomType, roomIndex);
    }

    public RoomType GetRoomType(int roomIndex)
    {
        return GetRoomType(roomIndex, TotalRooms);
    }

    public RoomType GetRoomType(int roomIndex, int runTotalRooms)
    {
        int safeTotalRooms = Mathf.Max(1, runTotalRooms);
        int roomNumber = roomIndex + 1;

        if (roomNumber >= safeTotalRooms)
            return RoomType.Final;

        if (bossEveryRooms > 0 && roomNumber % bossEveryRooms == 0)
            return RoomType.Boss;

        if (restEveryRooms > 0 && roomNumber % restEveryRooms == 0)
            return RoomType.Rest;

        if (treasureEveryRooms > 0 && roomNumber % treasureEveryRooms == 0)
            return RoomType.Treasure;

        if (eliteEveryRooms > 0 && roomNumber % eliteEveryRooms == 0)
            return RoomType.Elite;

        int pattern = roomNumber % 3;

        if (pattern == 0)
            return RoomType.Classroom;

        if (pattern == 1)
            return RoomType.OpenArea;

        return RoomType.Hallway;
    }

    private RoomData GetRoomFromType(RoomType roomType, int roomIndex)
    {
        switch (roomType)
        {
            case RoomType.Classroom:
                return Pick(classroomRooms, roomIndex, normalRooms);

            case RoomType.Hallway:
                return Pick(hallwayRooms, roomIndex, normalRooms);

            case RoomType.OpenArea:
                return Pick(openAreaRooms, roomIndex, normalRooms);

            case RoomType.Elite:
                return Pick(eliteRooms, roomIndex, normalRooms);

            case RoomType.Treasure:
                return Pick(treasureRooms, roomIndex, normalRooms);

            case RoomType.Rest:
                return Pick(restRooms, roomIndex, normalRooms);

            case RoomType.Boss:
                return Pick(bossRooms, roomIndex, normalRooms);

            case RoomType.Final:
                return Pick(finalRooms, roomIndex, bossRooms, normalRooms);

            default:
                return Pick(normalRooms, roomIndex);
        }
    }

    private RoomData Pick(RoomData[] primary, int roomIndex, params RoomData[][] fallbacks)
    {
        if (primary != null && primary.Length > 0)
            return primary[Mathf.Abs(roomIndex) % primary.Length];

        foreach (RoomData[] fallback in fallbacks)
        {
            if (fallback != null && fallback.Length > 0)
                return fallback[Mathf.Abs(roomIndex) % fallback.Length];
        }

        return null;
    }
}
