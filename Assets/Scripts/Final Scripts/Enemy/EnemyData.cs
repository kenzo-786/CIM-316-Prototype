using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    public GameObject prefab;

    [Header("Core Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 2.5f;
    public float contactDamage = 1f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int xpDropAmount = 1;

    [Header("Optional Projectile")]
    public EnemyProjectileData projectileData;

    [Header("Optional Spawn")]
    public EnemyData childEnemyData;
    public int childCount = 2;
}
