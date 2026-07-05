using UnityEngine;

public struct DamageInfo
{
    public float damage;
    public GameObject source;
    public Vector2 hitPoint;

    public DamageInfo(float damage, GameObject source, Vector2 hitPoint)
    {
        this.damage = damage;
        this.source = source;
        this.hitPoint = hitPoint;
    }
}
