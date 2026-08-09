using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class ShooterEnemy : EnemyBase
{
    [Header("Positioning")]
    [SerializeField] private float minimumDistance = 5f;
    [SerializeField] private float movementStepDuration = 0.35f;
    [SerializeField] private float movementStepCooldown = 0.8f;
    [SerializeField] private LayerMask shotBlockingLayer;

    [Header("Shot Telegraph")]
    [SerializeField, Min(0f)] private float shotWindup = 0.25f;
    [SerializeField] private EnemyTelegraphFeedback telegraph;

    [Header("Emergency Relocation")]
    [SerializeField] private bool allowRelocation = true;
    [SerializeField] private float dangerRange = 2.5f;
    [SerializeField] private float relocationCooldown = 4f;
    [SerializeField] private float relocationDelay = 0.45f;

    private EnemyProjectileShooter shooter;
    private RoomBounds roomBounds;
    private float nextShootTime;
    private float nextMovementTime;
    private float movementTimer;
    private Vector2 movementDirection;
    private bool moving;
    private float nextRelocateTime;
    private float relocationTimer;
    private bool relocating;
    private int sideStepDirection = 1;
    private bool preparingShot;
    private float shotTimer;

    protected override void Awake()
    {
        base.Awake();

        shooter = GetComponent<EnemyProjectileShooter>();
        roomBounds = FindObjectOfType<RoomBounds>();

        if (telegraph == null)
        {
            telegraph = GetComponent<EnemyTelegraphFeedback>();
        }

        sideStepDirection =
            GetInstanceID() % 2 == 0
                ? 1
                : -1;
    }

    public override void Initialize(
        EnemyData data,
        Transform playerTarget
    )
    {
        base.Initialize(data, playerTarget);

        roomBounds = FindObjectOfType<RoomBounds>();

        if (shooter != null &&
            data != null &&
            data.projectileData != null)
        {
            shooter.SetProjectileData(data.projectileData);
        }
    }

    protected override void TickEnemy()
    {
        if (target == null)
        {
            return;
        }

        if (preparingShot)
        {
            StopMoving();
            shotTimer -= Time.fixedDeltaTime;

            if (shotTimer <= 0f)
            {
                ReleaseShot();
            }

            return;
        }

        if (relocating)
        {
            TickRelocation();
            return;
        }

        if (moving)
        {
            TickMovementStep();
            return;
        }

        StopMoving();

        Vector2 toPlayer =
            (Vector2)target.position - rb.position;

        float distance = toPlayer.magnitude;

        if (allowRelocation &&
            distance <= dangerRange &&
            Time.time >= nextRelocateTime)
        {
            StartRelocation();
            return;
        }

        if (Time.time >= nextMovementTime)
        {
            if (distance < minimumDistance)
            {
                StartMovementStep(-toPlayer.normalized);
                return;
            }

            if (!HasClearShot())
            {
                Vector2 towardPlayer = toPlayer.normalized;

                Vector2 sideways =
                    new Vector2(
                        -towardPlayer.y,
                        towardPlayer.x
                    ) * sideStepDirection;

                sideStepDirection *= -1;

                StartMovementStep(sideways);
                return;
            }
        }

        if (HasClearShot() &&
            Time.time >= nextShootTime)
        {
            BeginShot();
        }
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

    private void BeginShot()
    {
        preparingShot = true;
        shotTimer = shotWindup;

        if (telegraph != null)
        {
            telegraph.Begin(shotWindup);
        }
    }

    private void ReleaseShot()
    {
        preparingShot = false;

        if (telegraph != null)
        {
            telegraph.End();
        }

        nextShootTime = Time.time + AttackCooldown;

        if (shooter != null &&
            target != null &&
            HasClearShot())
        {
            shooter.ShootAt(
                target.position,
                CurrentDifficulty.damageMultiplier,
                gameObject
            );
        }
    }

    private void StartMovementStep(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return;
        }

        moving = true;
        movementDirection = direction.normalized;
        movementTimer = movementStepDuration;

        nextMovementTime =
            Time.time +
            movementStepDuration +
            movementStepCooldown;
    }

    private void TickMovementStep()
    {
        movementTimer -= Time.fixedDeltaTime;

        Vector2 destination =
            rb.position +
            movementDirection *
            MoveSpeed *
            Time.fixedDeltaTime;

        if (roomBounds != null)
        {
            destination =
                roomBounds.ClampPoint(destination);
        }

        rb.MovePosition(destination);
        FaceDirection(movementDirection);

        if (movementTimer <= 0f)
        {
            moving = false;
            StopMoving();
        }
    }

    private void StartRelocation()
    {
        relocating = true;
        moving = false;
        relocationTimer = relocationDelay;

        nextRelocateTime =
            Time.time + relocationCooldown;

        if (telegraph != null)
        {
            telegraph.Begin(relocationDelay);
        }

        StopMoving();
    }

    private void TickRelocation()
    {
        StopMoving();

        relocationTimer -= Time.fixedDeltaTime;

        if (relocationTimer <= 0f)
        {
            relocating = false;

            if (telegraph != null)
            {
                telegraph.End();
            }

            if (roomBounds != null)
            {
                rb.position = roomBounds.GetRandomPoint();
            }
        }
    }
}
