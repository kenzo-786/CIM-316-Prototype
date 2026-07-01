using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Rooms/Room Data")]
public class RoomData : ScriptableObject
{
    public string roomName;
    public RoomType roomType;
    public RoomLayout layoutPrefab;
}
