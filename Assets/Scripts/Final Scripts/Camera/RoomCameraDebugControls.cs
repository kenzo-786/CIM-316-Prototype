using UnityEngine;

public class RoomCameraDebugControls : MonoBehaviour
{
    [SerializeField] private RoomCameraController roomCamera;

    private void Update()
    {
        if (roomCamera == null) return;

        if (Input.GetKeyDown(KeyCode.N))
            roomCamera.MoveToNextRoom();

        if (Input.GetKeyDown(KeyCode.B))
            roomCamera.MoveToRoom(roomCamera.CurrentRoomIndex - 1);
    }
}
