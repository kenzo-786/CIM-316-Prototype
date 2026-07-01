using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RoomCameraController : MonoBehaviour
{
    [SerializeField] private RoomCameraSettings settings;
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private int startingRoomIndex = 0;

    private Camera cam;
    private Vector3 targetPosition;
    private int currentRoomIndex;

    public int CurrentRoomIndex => currentRoomIndex;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;

        if (settings != null)
            cam.orthographicSize = settings.orthographicSize;

        SnapToRoom(startingRoomIndex);
    }

    private void LateUpdate()
    {
        if (!smoothTransition) return;

        float speed = settings != null ? settings.transitionSpeed : 8f;
        float step = 1f - Mathf.Exp(-speed * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, targetPosition, step);
    }

    public void MoveToRoom(int roomIndex)
    {
        currentRoomIndex = Mathf.Max(0, roomIndex);
        targetPosition = GetRoomCameraPosition(currentRoomIndex);

        if (!smoothTransition)
            transform.position = targetPosition;
    }

    public void MoveToNextRoom()
    {
        MoveToRoom(currentRoomIndex + 1);
    }

    public void SnapToRoom(int roomIndex)
    {
        currentRoomIndex = Mathf.Max(0, roomIndex);
        targetPosition = GetRoomCameraPosition(currentRoomIndex);
        transform.position = targetPosition;
    }

    private Vector3 GetRoomCameraPosition(int roomIndex)
    {
        Vector2 roomSize = settings != null ? settings.roomSize : new Vector2(32f, 18f);
        float z = settings != null ? settings.cameraZ : -10f;

        return new Vector3(roomIndex * roomSize.x, 0f, z);
    }
}
