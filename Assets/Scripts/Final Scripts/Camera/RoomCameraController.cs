using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RoomCameraController : MonoBehaviour
{
    [SerializeField] private RoomCameraSettings settings;
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private int startingRoomIndex = 0;

    private Camera cam;
    private Vector3 currentRoomPosition;
    private Vector3 targetRoomPosition;
    private Vector3 shakeOffset;
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
        if (smoothTransition)
        {
            float speed =
                settings != null
                    ? settings.transitionSpeed
                    : 8f;

            float step =
                1f -
                Mathf.Exp(
                    -speed * Time.deltaTime);

            currentRoomPosition =
                Vector3.Lerp(
                    currentRoomPosition,
                    targetRoomPosition,
                    step);
        }
        else
        {
            currentRoomPosition =
                targetRoomPosition;
        }

        ApplyFinalPosition();
    }

    public void MoveToRoom(int roomIndex)
    {
        currentRoomIndex =
           Mathf.Max(0, roomIndex);

        targetRoomPosition =
            GetRoomCameraPosition(
                currentRoomIndex);

        if (!smoothTransition)
        {
            currentRoomPosition =
                targetRoomPosition;

            ApplyFinalPosition();
        }
    }

    public void MoveToNextRoom()
    {
        MoveToRoom(currentRoomIndex + 1);
    }

    public void SnapToRoom(int roomIndex)
    {
        currentRoomIndex =
            Mathf.Max(0, roomIndex);

        targetRoomPosition =
            GetRoomCameraPosition(
                currentRoomIndex);

        currentRoomPosition =
            targetRoomPosition;

        ApplyFinalPosition();
    }

    public void SetShakeOffset(Vector2 offset)
    {
        shakeOffset =
            new Vector3(
                offset.x,
                offset.y,
                0f);
    }

    public void ClearShakeOffset()
    {
        shakeOffset = Vector3.zero;
    }

    private void ApplyFinalPosition()
    {
        transform.position =
            currentRoomPosition +
            shakeOffset;
    }

    private Vector3 GetRoomCameraPosition(int roomIndex)
    {
        Vector2 roomSize = settings != null ? settings.roomSize : new Vector2(32f, 18f);
        float z = settings != null ? settings.cameraZ : -10f;

        return new Vector3(roomIndex * roomSize.x, 0f, z);
    }
}
