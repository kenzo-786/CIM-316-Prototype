using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.05f;
    private Vector3 _velocity = Vector3.zero;

    void Start()
    {
        transform.SetParent(null);
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);

        transform.rotation = Quaternion.identity;
    }
}
