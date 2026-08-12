using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class MageEnemy : EnemyBase
{
    [Header("Mage")]
    [SerializeField] private float preferredRange = 7f;
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float spreadAngle = 25f;
    [SerializeField] private float castWindup = 0.5f;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private EnemyProjectileShooter shooter;
    private float nextCastTime;
    private bool casting;
    private float castTimer;
    private Vector2 castDirection = Vector2.down;

    protected override void Awake()
    {
        base.Awake();

        shooter = GetComponent<EnemyProjectileShooter>();

        if (telegraph == null)
            telegraph = GetComponent<EnemyTelegraphFeedback>();

        if (animationController == null)
            animationController = GetComponentInChildren<EnemyAnimationController>();
    }

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);

        if (shooter != null && data != null && data.projectileData != null)
            shooter.SetProjectileData(data.projectileData);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        casting = false;
        animationController?.SetStationary();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (casting)
        {
            StopMoving();
            animationController?.SetStationary();
            animationController?.SetFacingDirection(castDirection);

            castTimer -= Time.fixedDeltaTime;

            if (castTimer <= 0f)
                ReleaseCast();

            return;
        }

        Vector2 toPlayer = (Vector2)target.position - rb.position;
        float distance = toPlayer.magnitude;
        Vector2 movementDirection = Vector2.zero;

        if (distance > 0.01f)
        {
            if (distance < preferredRange * 0.7f)
                movementDirection = -toPlayer.normalized;
            else if (distance > preferredRange)
                movementDirection = toPlayer.normalized;
        }

        if (movementDirection != Vector2.zero)
        {
            MoveInDirection(movementDirection);
            animationController?.SetMovementDirection(movementDirection);
        }
        else
        {
            StopMoving();
            animationController?.SetStationary();
            animationController?.SetFacingDirection(toPlayer);
        }

        if (Time.time >= nextCastTime)
            StartCast(toPlayer);
    }

    private void StartCast(Vector2 toPlayer)
    {
        if (toPlayer.sqrMagnitude > 0.0001f)
            castDirection = toPlayer.normalized;

        casting = true;
        castTimer = castWindup;
        nextCastTime = Time.time + AttackCooldown;

        StopMoving();
        animationController?.SetStationary();
        animationController?.SetFacingDirection(castDirection);
        animationController?.PlayAttack();

        if (telegraph != null)
            telegraph.Begin(castWindup);
    }

    private void ReleaseCast()
    {
        casting = false;

        if (telegraph != null)
            telegraph.End();

        if (shooter != null)
        {
            shooter.ShootSpread(
                castDirection,
                projectileCount,
                spreadAngle,
                CurrentDifficulty.damageMultiplier,
                gameObject
            );
        }
    }
}
