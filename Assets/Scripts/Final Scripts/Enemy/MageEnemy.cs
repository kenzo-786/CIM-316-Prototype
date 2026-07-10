using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class MageEnemy : EnemyBase
{
    [Header("Mage")]
    [SerializeField] private float preferredRange = 7f;
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float spreadAngle = 25f;
    [SerializeField] private float castWindup = 0.45f;

    private EnemyProjectileShooter shooter;
    private float nextCastTime;
    private bool casting;
    private float castTimer;

    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<EnemyProjectileShooter>();
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

        if (casting)
        {
            StopMoving();
            castTimer -= Time.fixedDeltaTime;

            if (castTimer <= 0f)
                ReleaseCast();

            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance < preferredRange * 0.7f)
            MoveAwayFrom(target.position);
        else if (distance > preferredRange)
            MoveToward(target.position);
        else
            StopMoving();

        if (Time.time >= nextCastTime)
            StartCast();
    }

    private void StartCast()
    {
        casting = true;
        castTimer = castWindup;
        nextCastTime = Time.time + AttackCooldown;
    }

    private void ReleaseCast()
    {
        casting = false;

        Vector2 baseDirection = ((Vector2)target.position - rb.position).normalized;

        if (shooter != null)
            shooter.ShootSpread(baseDirection, projectileCount, spreadAngle, CurrentDifficulty.damageMultiplier, gameObject);
    }

}
