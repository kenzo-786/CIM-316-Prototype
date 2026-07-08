using UnityEngine;

public class RoomBounds : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(30f, 16f);
    [SerializeField] private float edgePadding = 2f;
    [SerializeField] private LayerMask blockedLayer;

    public Vector3 GetRandomPoint()
    {
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(
                transform.position.x - size.x * 0.5f + edgePadding,
                transform.position.x + size.x * 0.5f - edgePadding
            );

            float y = Random.Range(
                transform.position.y - size.y * 0.5f + edgePadding,
                transform.position.y + size.y * 0.5f - edgePadding
            );

            Vector3 point = new Vector3(x, y, 0f);

            if (!Physics2D.OverlapCircle(point, 0.5f, blockedLayer))
                return point;
        }

        return transform.position;
    }
}
