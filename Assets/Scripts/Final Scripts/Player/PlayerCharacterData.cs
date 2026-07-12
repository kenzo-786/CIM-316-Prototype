using UnityEngine;

public enum PlayerWeaponType
{
    Ruler,
    Eraser
}

[CreateAssetMenu(menuName = "Deadline Dungeon/Player Character")]
public class PlayerCharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public Sprite portrait;
    public Sprite gameplaySprite;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;

    [Header("Combat")]
    public PlayerWeaponType weaponType;
    public bool canAttackWhileMoving;
}
