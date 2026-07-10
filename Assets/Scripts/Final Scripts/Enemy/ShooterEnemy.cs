using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class ShooterEnemy : EnemyBase
{
    [Header("Shooter")]
    [SerializeField] private float preferredRange = 8f;
    [SerializeField] private float dangerRange = 3f;
    [SerializeField] private float relocationCooldown = 3f;
    [SerializeField] private float relocationDelay = 0.45f;

    private EnemyProjectileShooter shooter;
    private RoomBounds roomBounds;
    private float nextShootTime;
    private float nextRelocateTime;
    private float relocateTimer;
    private bool relocating;

    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<EnemyProjectileShooter>();
        roomBounds = FindObjectOfType<RoomBounds>();
    }

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);

        if (shooter != null && data != null && data.projectileData != null)
            shooter.SetProjectileData(data.projectileData);
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (relocating)
        {
            StopMoving();
            relocateTimer -= Time.fixedDeltaTime;

            if (relocateTimer <= 0f)
                FinishRelocation();

            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= dangerRange && Time.time >= nextRelocateTime)
        {
            StartRelocation();
            return;
        }

        if (distance < preferredRange * 0.75f)
            MoveAwayFrom(target.position);
        else if (distance > preferredRange)
            MoveToward(target.position);
        else
            StopMoving();

        TryShoot();
    }

    private void TryShoot()
    {
        if (Time.time < nextShootTime)
            return;

        nextShootTime = Time.time + AttackCooldown;

        if (shooter != null)
            shooter.ShootAt(target.position, CurrentDifficulty.damageMultiplier, gameObject);
    }

    private void StartRelocation()
    {
        relocating = true;
        relocateTimer = relocationDelay;
        nextRelocateTime = Time.time + relocationCooldown;
    }

    private void FinishRelocation()
    {
        relocating = false;

        if (roomBounds != null)
            transform.position = roomBounds.GetRandomPoint();
    }
}
