using UnityEngine;
[CreateAssetMenu(menuName = "Deadline Dungeon/Camera/Room Camera Settings")]

public class RoomCameraSettings : ScriptableObject
{
    public Vector2 roomSize = new Vector2(32f, 18f);
    public float orthographicSize = 9f;
    public float cameraZ = -10f;
    public float transitionSpeed = 8f;
}
