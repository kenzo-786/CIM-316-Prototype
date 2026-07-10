using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Enemies/Enemy Projectile Data")]
public class EnemyProjectileData : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Stats")]
    public float damage = 1f;
    public float speed = 8f;
    public float lifetime = 5f;
    public int wallBounces = 0;
    public int hitsBeforeDestroy = 1;
    public bool destroyOnPlayerHit = true;

    [Header("Collision")]
    public LayerMask playerLayer;
    public LayerMask wallLayer;
    public LayerMask destroyLayer;

    [Header("Visual")]
    public bool rotateToDirection = true;
}
