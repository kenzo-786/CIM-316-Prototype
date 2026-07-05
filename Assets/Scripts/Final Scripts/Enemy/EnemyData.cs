using UnityEngine;

[CreateAssetMenu(menuName = "Deadline Dungeon/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public GameObject prefab;

    [Header("Stats")]
    public float maxHealth = 30f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("Attack")]
    public float attackRange = 1.1f;
    public float attackCooldown = 1f;

    [Header("Rewards")]
    public int xpDropAmount = 1;
}
