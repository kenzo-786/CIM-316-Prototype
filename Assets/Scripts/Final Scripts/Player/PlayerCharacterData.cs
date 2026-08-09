using UnityEngine;

public enum PlayerWeaponType
{
    Ruler,
    Eraser
}

public enum PlayerFiringMode
{
    BuildAStationaryMouseAim,
    BuildBMoveAndShootMouseAim,
    BuildCStationaryAutoTarget
}

[CreateAssetMenu(menuName = "Deadline Dungeon/Player Character")]
public class PlayerCharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public Sprite portrait;
    public Sprite gameplaySprite;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;

    [Header("Combat")]
    public PlayerWeaponType weaponType;

    public PlayerFiringMode firingMode =
        PlayerFiringMode.BuildAStationaryMouseAim;
}
