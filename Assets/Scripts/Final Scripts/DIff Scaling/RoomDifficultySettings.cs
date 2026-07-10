using UnityEngine;

[System.Serializable]
public struct RoomDifficultySnapshot
{
    public float healthMultiplier;
    public float damageMultiplier;
    public float moveSpeedMultiplier;

    public static RoomDifficultySnapshot Default => new RoomDifficultySnapshot
    {
        healthMultiplier = 1f,
        damageMultiplier = 1f,
        moveSpeedMultiplier = 1f
    };
}

[CreateAssetMenu(menuName = "Deadline Dungeon/Run/Room Difficulty Settings")]
public class RoomDifficultySettings : ScriptableObject
{
    [SerializeField] private float healthGrowthPerRoom = 0.08f;
    [SerializeField] private float damageGrowthPerRoom = 0.04f;
    [SerializeField] private float speedGrowthPerRoom = 0.015f;

    [SerializeField] private float maxHealthMultiplier = 4f;
    [SerializeField] private float maxDamageMultiplier = 3f;
    [SerializeField] private float maxSpeedMultiplier = 1.5f;

    public RoomDifficultySnapshot GetDifficulty(int roomIndex)
    {
        int roomNumber = roomIndex + 1;

        return new RoomDifficultySnapshot
        {
            healthMultiplier = Mathf.Min(1f + healthGrowthPerRoom * roomNumber, maxHealthMultiplier),
            damageMultiplier = Mathf.Min(1f + damageGrowthPerRoom * roomNumber, maxDamageMultiplier),
            moveSpeedMultiplier = Mathf.Min(1f + speedGrowthPerRoom * roomNumber, maxSpeedMultiplier)
        };
    }
}
