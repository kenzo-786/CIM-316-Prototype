using UnityEngine;

public enum DamageType
{
    Normal,
    Critical,
    Headshot,
    OneShot
}

public struct DamageRoll
{
    public float damage;
    public DamageType damageType;

    public DamageRoll(float damage, DamageType damageType)
    {
        this.damage = damage;
        this.damageType = damageType;
    }
}

public struct DamageInfo
{
    public float damage;
    public GameObject source;
    public Vector2 hitPoint;
    public DamageType damageType;

    public DamageInfo(
        float damage,
        GameObject source,
        Vector2 hitPoint,
        DamageType damageType = DamageType.Normal)
    {
        this.damage = damage;
        this.source = source;
        this.hitPoint = hitPoint;
        this.damageType = damageType;
    }
}
