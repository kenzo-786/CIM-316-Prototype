using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Enemies/Enemy Projectile Data")]
public class EnemyProjectileData : ScriptableObject
{
    public GameObject prefab;
    public float damage = 10f;
    public float speed = 7f;
    public float lifetime = 4f;
    public int wallBounces;
    public LayerMask playerLayer;
    public LayerMask wallLayer;
    public LayerMask destroyLayer;
}
