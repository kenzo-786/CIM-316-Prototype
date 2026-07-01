using UnityEngine;

public enum PlayerWeaponType
{
    Ruler,
    Eraser
}

[CreateAssetMenu(menuName = "Deadline Dungeon/Player Character")]
public class PlayerCharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;

    public float maxHealth = 100f;
    public float moveSpeed = 5f;

    public PlayerWeaponType weaponType;
    public bool canAttackWhileMoving = false;
}
