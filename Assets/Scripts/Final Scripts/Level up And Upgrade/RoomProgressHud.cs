using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RoomProgressHud : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private TMP_Text roomText;

    private void OnEnable()
    {
        roomManager.OnRoomChanged += UpdateUI;
    }

    private void OnDisable()
    {
        roomManager.OnRoomChanged -= UpdateUI;
    }

    private void UpdateUI(int currentRoom, int totalRooms)
    {
        if (roomText != null)
            roomText.text = "Room " + currentRoom + " / " + totalRooms;
    }
}
