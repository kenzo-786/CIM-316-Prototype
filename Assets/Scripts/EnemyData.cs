using UnityEngine;

public enum EnemyBehavior { Chaser, Dasher, Tank, Swarmer }

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "DeadlineDungeon/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject enemyPrefab;
    public EnemyBehavior behaviorType;

    [Header("Base Stats")]
    public float maxHealth = 10f;
    public float damage = 5f;
    public float moveSpeed = 3f;

    [Header("Dasher Specific")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;
    public Color telegraphColor = Color.white;
}
