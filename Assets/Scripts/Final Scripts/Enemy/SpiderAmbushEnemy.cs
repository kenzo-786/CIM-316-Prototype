using UnityEngine;
using System.Collections;

public class SpiderAmbushEnemy : EnemyBase
{
    private enum SpiderState
    {
        SpawnReveal,
        Hidden,
        Telegraphing,
        Pouncing,
        Attacking,
        Vanishing
    }

    [Header("Initial Spawn")]
    [SerializeField] private float spawnVisibleDuration = 1.2f;
    [SerializeField] private float initialFlashDuration = 0.4f;

    [Header("Autonomous Hunt Cycle")]
    [SerializeField] private float hiddenDurationMin = 1.6f;
    [SerializeField] private float hiddenDurationMax = 2.6f;
    [SerializeField] private float emergeDistanceMin = 2.8f;
    [SerializeField] private float emergeDistanceMax = 4.2f;
    [SerializeField] private float telegraphDuration = 0.65f;
    [SerializeField] private float pounceDuration = 1.75f;
    [SerializeField] private float pounceSpeedMultiplier = 1.4f;
    [SerializeField] private float attackAfterEmergingDelay = 0.15f;

    [Header("Attack")]
    [SerializeField] private float attackAnimationDuration = 0.67f;
    [SerializeField] private float attackHitDelay = 0.33f;
    [SerializeField] private float attackHitRangeMultiplier = 1.15f;

    [Header("Disappearing")]
    [SerializeField] private float vanishDuration = 0.35f;
    [SerializeField] private float flashInterval = 0.08f;

    [Header("Safe Placement")]
    [SerializeField] private float spawnClearance = 0.45f;
    [SerializeField] private float roomEdgePadding = 1.5f;
    [SerializeField] private float minimumReemergeDistance = 2.5f;
    [SerializeField] private LayerMask blockedLayer;

    [Header("References")]
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private SpriteRenderer[] visuals;
    [SerializeField] private EnemyTelegraphFeedback telegraph;
    [SerializeField] private EnemyAnimationController animationController;

    private RoomBounds roomBounds;
    private SpiderState state;
    private float stateTimer;
    private float attackReadyTime;
    private float attackHitTime;
    private float bodyRadius;
    private bool attackDamageApplied;
    private bool hasLastVanishPosition;
    private Vector2 lastVanishPosition;
    private Vector2 lastAttackDirection = Vector2.down;

    protected override void Awake()
    {
        base.Awake();

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (visuals == null || visuals.Length == 0)
            visuals = GetComponentsInChildren<SpriteRenderer>();

        if (telegraph == null)
            telegraph = GetComponent<EnemyTelegraphFeedback>();

        if (animationController == null)
            animationController = GetComponentInChildren<EnemyAnimationController>();

        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            bodyRadius = Mathf.Max(bounds.extents.x, bounds.extents.y);
        }

        bodyRadius = Mathf.Max(bodyRadius, 0.25f);
        roomBounds = FindObjectOfType<RoomBounds>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        roomBounds = FindObjectOfType<RoomBounds>();
        hasLastVanishPosition = false;

        BeginSpawnReveal();
    }

    public override void Initialize(
        EnemyData data,
        Transform playerTarget
    )
    {
        base.Initialize(data, playerTarget);

        roomBounds = FindObjectOfType<RoomBounds>();
        hasLastVanishPosition = false;

        BeginSpawnReveal();
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        switch (state)
        {
            case SpiderState.SpawnReveal:
                TickSpawnReveal();
                break;

            case SpiderState.Hidden:
                TickHidden();
                break;

            case SpiderState.Telegraphing:
                TickTelegraph();
                break;

            case SpiderState.Pouncing:
                TickPouncing();
                break;

            case SpiderState.Attacking:
                TickAttacking();
                break;

            case SpiderState.Vanishing:
                TickVanishing();
                break;
        }
    }

    private void TickSpawnReveal()
    {
        StopMoving();
        animationController?.SetStationary();

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer <= initialFlashDuration)
            SetVisuals(IsFlashVisible());
        else
            SetVisuals(true);

        if (stateTimer <= 0f)
            EnterHidden();
    }

    private void TickHidden()
    {
        StopMoving();
        animationController?.SetStationary();

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer <= 0f)
            BeginTelegraph();
    }

    private void TickTelegraph()
    {
        StopMoving();
        animationController?.SetStationary();

        stateTimer -= Time.fixedDeltaTime;
        SetVisuals(IsFlashVisible());

        if (stateTimer <= 0f)
            BeginPounce();
    }

    private void TickPouncing()
    {
        stateTimer -= Time.fixedDeltaTime;

        Vector2 directionToPlayer =
            (Vector2)target.position - rb.position;

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            lastAttackDirection = directionToPlayer.normalized;

            MoveInDirection(
                directionToPlayer,
                pounceSpeedMultiplier
            );

            animationController?.SetMovementDirection(
                directionToPlayer
            );
        }

        if (Time.time >= attackReadyTime &&
            IsTargetInRange(AttackRange))
        {
            BeginAttack();
            return;
        }

        if (stateTimer <= 0f)
            BeginVanishing();
    }

    private void TickAttacking()
    {
        StopMoving();
        animationController?.SetStationary();
        animationController?.SetFacingDirection(
            lastAttackDirection
        );

        stateTimer -= Time.fixedDeltaTime;

        if (!attackDamageApplied &&
            Time.time >= attackHitTime)
        {
            attackDamageApplied = true;

            if (IsTargetInRange(
                    AttackRange *
                    attackHitRangeMultiplier
                ))
            {
                DamageTarget(
                    ContactDamage,
                    target.position
                );
            }
        }

        if (stateTimer <= 0f)
            BeginVanishing();
    }

    private void TickVanishing()
    {
        StopMoving();
        animationController?.SetStationary();

        stateTimer -= Time.fixedDeltaTime;
        SetVisuals(IsFlashVisible());

        if (stateTimer <= 0f)
            EnterHidden();
    }

    private void BeginSpawnReveal()
    {
        state = SpiderState.SpawnReveal;
        stateTimer = spawnVisibleDuration;

        SetVisuals(true);
        SetBodyCollider(true);
        animationController?.SetStationary();
    }

    private void EnterHidden()
    {
        state = SpiderState.Hidden;

        stateTimer = Random.Range(
            hiddenDurationMin,
            hiddenDurationMax
        );

        telegraph?.End();

        SetVisuals(false);
        SetBodyCollider(false);
        StopMoving();
        animationController?.SetStationary();
    }

    private void BeginTelegraph()
    {
        state = SpiderState.Telegraphing;
        stateTimer = telegraphDuration;

        transform.position = FindSafeAmbushPosition();

        SetBodyCollider(false);
        SetVisuals(true);
        StopMoving();
        animationController?.SetStationary();

        if (telegraph != null)
            telegraph.Begin(telegraphDuration);
    }

    private void BeginPounce()
    {
        state = SpiderState.Pouncing;
        stateTimer = pounceDuration;
        attackReadyTime =
            Time.time + attackAfterEmergingDelay;

        telegraph?.End();

        SetVisuals(true);
        SetBodyCollider(true);
    }

    private void BeginAttack()
    {
        state = SpiderState.Attacking;
        stateTimer = attackAnimationDuration;
        attackHitTime = Time.time + attackHitDelay;
        attackDamageApplied = false;

        StopMoving();

        animationController?.SetStationary();
        animationController?.SetFacingDirection(
            lastAttackDirection
        );
        animationController?.PlayAttack();
    }

    private void BeginVanishing()
    {
        if (state == SpiderState.Hidden ||
            state == SpiderState.Vanishing)
        {
            return;
        }

        lastVanishPosition = rb.position;
        hasLastVanishPosition = true;

        state = SpiderState.Vanishing;
        stateTimer = vanishDuration;

        telegraph?.End();

        SetBodyCollider(false);
        StopMoving();
        animationController?.SetStationary();
    }

    private Vector2 FindSafeAmbushPosition()
    {
        if (roomBounds == null)
            return rb.position;

        for (int attempt = 0; attempt < 32; attempt++)
        {
            Vector2 direction =
                Random.insideUnitCircle.normalized;

            if (direction == Vector2.zero)
                direction = Vector2.right;

            float distance = Random.Range(
                emergeDistanceMin,
                emergeDistanceMax
            );

            Vector2 candidate =
                (Vector2)target.position +
                direction * distance;

            candidate = ClampInsideRoom(candidate);

            if (Vector2.Distance(
                    candidate,
                    target.position
                ) < emergeDistanceMin * 0.65f)
            {
                continue;
            }

            if (hasLastVanishPosition &&
                Vector2.Distance(
                    candidate,
                    lastVanishPosition
                ) < minimumReemergeDistance)
            {
                continue;
            }

            float requiredClearance =
                spawnClearance + bodyRadius;

            bool blocked = Physics2D.OverlapCircle(
                candidate,
                requiredClearance,
                blockedLayer
            );

            if (!blocked)
                return candidate;
        }

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 candidate = ClampInsideRoom(
                roomBounds.GetRandomPoint()
            );

            bool closeToLastPosition =
                hasLastVanishPosition &&
                Vector2.Distance(
                    candidate,
                    lastVanishPosition
                ) < minimumReemergeDistance;

            bool blocked = Physics2D.OverlapCircle(
                candidate,
                spawnClearance + bodyRadius,
                blockedLayer
            );

            if (!closeToLastPosition && !blocked)
                return candidate;
        }

        Vector2 fallbackDirection =
            hasLastVanishPosition
                ? ((Vector2)target.position -
                   lastVanishPosition).normalized
                : Vector2.right;

        if (fallbackDirection == Vector2.zero)
            fallbackDirection = Vector2.right;

        return ClampInsideRoom(
            (Vector2)target.position +
            fallbackDirection *
            emergeDistanceMax
        );
    }

    private Vector2 ClampInsideRoom(Vector2 point)
    {
        if (roomBounds == null)
            return point;

        float halfWidth = Mathf.Max(
            0.1f,
            roomBounds.Size.x * 0.5f -
            roomEdgePadding -
            bodyRadius
        );

        float halfHeight = Mathf.Max(
            0.1f,
            roomBounds.Size.y * 0.5f -
            roomEdgePadding -
            bodyRadius
        );

        return new Vector2(
            Mathf.Clamp(
                point.x,
                roomBounds.Center.x - halfWidth,
                roomBounds.Center.x + halfWidth
            ),
            Mathf.Clamp(
                point.y,
                roomBounds.Center.y - halfHeight,
                roomBounds.Center.y + halfHeight
            )
        );
    }

    private bool IsFlashVisible()
    {
        if (flashInterval <= 0f)
            return true;

        return Mathf.FloorToInt(
            Time.time / flashInterval
        ) % 2 == 0;
    }

    private void SetVisuals(bool visible)
    {
        if (visuals == null)
            return;

        foreach (SpriteRenderer spriteRenderer in visuals)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
        }
    }

    private void SetBodyCollider(bool enabled)
    {
        if (bodyCollider != null)
            bodyCollider.enabled = enabled;
    }
}
