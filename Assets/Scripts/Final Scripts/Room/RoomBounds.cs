using UnityEngine;

public class RoomBounds : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(30f, 16f);
    [SerializeField] private float padding = 1f;
    [SerializeField] private LayerMask blockedLayer;

    public Vector2 Center => transform.position;
    public Vector2 Size => size;

    public Vector2 GetRandomPoint()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 point = new Vector2(
                Random.Range(Center.x - size.x * 0.5f + padding, Center.x + size.x * 0.5f - padding),
                Random.Range(Center.y - size.y * 0.5f + padding, Center.y + size.y * 0.5f - padding)
            );

            if (!Physics2D.OverlapCircle(point, 0.35f, blockedLayer))
                return point;
        }

        return Center;
    }

    public Vector2 ClampPoint(Vector2 point)
    {
        float minX = Center.x - size.x * 0.5f + padding;
        float maxX = Center.x + size.x * 0.5f - padding;
        float minY = Center.y - size.y * 0.5f + padding;
        float maxY = Center.y + size.y * 0.5f - padding;

        return new Vector2(Mathf.Clamp(point.x, minX, maxX), Mathf.Clamp(point.y, minY, maxY));
    }
}
