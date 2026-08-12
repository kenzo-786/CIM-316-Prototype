using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class ShooterEnemy : EnemyBase
{
    [Header("Positioning")]
    [SerializeField] private float minimumDistance = 5.5f;
    [SerializeField] private float maximumDistance = 8.5f;
    [SerializeField] private float retreatSpeedMultiplier = 1.2f;
    [SerializeField] private float approachSpeedMultiplier = 0.7f;
    [SerializeField] private float strafeSpeedMultiplier = 0.6f;
    [SerializeField] private float strafeChangeInterval = 1.4f;
    [SerializeField] private LayerMask shotBlockingLayer;

    [Header("Shot")]
    [SerializeField] private float shotWindup = 0.5f;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private EnemyProjectileShooter shooter;
    private RoomBounds roomBounds;
    private float nextShootTime;
    private float shotTimer;
    private float nextStrafeChangeTime;
    private int strafeDirection;
    private bool preparingShot;
    private Vector2 shotDirection = Vector2.down;

    protected override void Awake()
    {
        base.Awake();

        shooter = GetComponent<EnemyProjectileShooter>();
        roomBounds = FindObjectOfType<RoomBounds>();

        if (telegraph == null)
            telegraph = GetComponent<EnemyTelegraphFeedback>();

        if (animationController == null)
            animationController = GetComponentInChildren<EnemyAnimationController>();
    }

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);

        roomBounds = FindObjectOfType<RoomBounds>();

        if (shooter != null && data != null && data.projectileData != null)
            shooter.SetProjectileData(data.projectileData);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        preparingShot = false;
        nextShootTime = 0f;
        strafeDirection = Random.value < 0.5f ? -1 : 1;
        nextStrafeChangeTime = Time.time + strafeChangeInterval;

        animationController?.SetStationary();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        if (preparingShot)
        {
            StopMoving();
            animationController?.SetStationary();
            animationController?.SetFacingDirection(shotDirection);

            shotTimer -= Time.fixedDeltaTime;

            if (shotTimer <= 0f)
                ReleaseShot();

            return;
        }

        Vector2 toPlayer = (Vector2)target.position - rb.position;

        if (toPlayer.sqrMagnitude <= 0.0001f)
        {
            StopMoving();
            return;
        }

        Vector2 directionToPlayer = toPlayer.normalized;
        float distance = toPlayer.magnitude;

        MaintainPosition(directionToPlayer, distance);

        if (HasClearShot() && Time.time >= nextShootTime)
            BeginShot(directionToPlayer);
    }

    private void MaintainPosition(Vector2 directionToPlayer, float distance)
    {
        animationController?.SetStationary();
        animationController?.SetFacingDirection(directionToPlayer);

        if (distance < minimumDistance)
        {
            MoveInDirection(-directionToPlayer, retreatSpeedMultiplier);
            return;
        }

        if (distance > maximumDistance)
        {
            MoveInDirection(directionToPlayer, approachSpeedMultiplier);
            return;
        }

        if (!HasClearShot())
        {
            if (Time.time >= nextStrafeChangeTime)
            {
                strafeDirection *= -1;
                nextStrafeChangeTime = Time.time + strafeChangeInterval;
            }

            Vector2 sideways = new Vector2(
                -directionToPlayer.y,
                directionToPlayer.x
            ) * strafeDirection;

            MoveInDirection(sideways, strafeSpeedMultiplier);
            return;
        }

        StopMoving();
    }

    private bool HasClearShot()
    {
        RaycastHit2D hit = Physics2D.Linecast(
            transform.position,
            target.position,
            shotBlockingLayer
        );

        return hit.collider == null;
    }

    private void BeginShot(Vector2 directionToPlayer)
    {
        preparingShot = true;
        shotTimer = shotWindup;
        shotDirection = directionToPlayer;

        StopMoving();
        animationController?.SetStationary();
        animationController?.SetFacingDirection(shotDirection);
        animationController?.PlayAttack();

        if (telegraph != null)
            telegraph.Begin(shotWindup);
    }

    private void ReleaseShot()
    {
        preparingShot = false;
        nextShootTime = Time.time + AttackCooldown;

        if (telegraph != null)
            telegraph.End();

        if (shooter != null)
        {
            shooter.ShootDirection(
                shotDirection,
                CurrentDifficulty.damageMultiplier,
                gameObject
            );
        }
    }
}
