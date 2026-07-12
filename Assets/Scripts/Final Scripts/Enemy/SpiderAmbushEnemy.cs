using UnityEngine;
using System.Collections;

public class SpiderAmbushEnemy : EnemyBase
{
    private enum SpiderState
    {
        SpawnReveal,
        Hidden,
        Telegraphing,
        Active,
        Vanishing
    }

    [Header("Initial Spawn")]
    [SerializeField] private float spawnVisibleDuration = 1.5f;
    [SerializeField] private float initialFlashDuration = 0.5f;

    [Header("Ambush")]
    [SerializeField] private float stationaryDelay = 0.7f;
    [SerializeField] private float emergeDistance = 2.2f;
    [SerializeField] private float telegraphDuration = 0.75f;
    [SerializeField] private float attackAfterEmergingDelay = 0.2f;

    [Header("Disappearing")]
    [SerializeField] private float vanishDistance = 5f;
    [SerializeField] private float vanishDuration = 0.4f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Placement")]
    [SerializeField] private float spawnClearance = 0.45f;
    [SerializeField] private LayerMask blockedLayer;

    [Header("References")]
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private SpriteRenderer[] visuals;

    private PlayerMovement playerMovement;
    private RoomBounds roomBounds;

    private SpiderState state;
    private float stateTimer;
    private float stationaryTimer;
    private float attackReadyTime;
    private float nextAttackTime;

    protected override void Awake()
    {
        base.Awake();

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (visuals == null || visuals.Length == 0)
            visuals = GetComponentsInChildren<SpriteRenderer>();

        roomBounds = FindObjectOfType<RoomBounds>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BeginSpawnReveal();
    }

    public override void Initialize(
        EnemyData data,
        Transform playerTarget)
    {
        base.Initialize(data, playerTarget);

        playerMovement = playerTarget != null
            ? playerTarget.GetComponent<PlayerMovement>()
            : null;

        roomBounds = FindObjectOfType<RoomBounds>();

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

            case SpiderState.Active:
                TickActive();
                break;

            case SpiderState.Vanishing:
                TickVanishing();
                break;
        }
    }

    private void TickSpawnReveal()
    {
        StopMoving();
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

        if (IsPlayerMoving())
        {
            stationaryTimer = 0f;
            return;
        }

        stationaryTimer += Time.fixedDeltaTime;

        if (stationaryTimer >= stationaryDelay)
            BeginTelegraph();
    }

    private void TickTelegraph()
    {
        StopMoving();
        stateTimer -= Time.fixedDeltaTime;

        SetVisuals(IsFlashVisible());

        // Moving during the warning cancels the ambush.
        if (IsPlayerMoving())
        {
            BeginVanishing();
            return;
        }

        if (stateTimer <= 0f)
            BecomeActive();
    }

    private void TickActive()
    {
        SetVisuals(true);
        SetBodyCollider(true);

        float distance =
            Vector2.Distance(transform.position, target.position);

        if (distance > vanishDistance)
        {
            BeginVanishing();
            return;
        }

        if (!IsTargetInRange(AttackRange))
        {
            MoveToward(target.position);
            return;
        }

        StopMoving();
        TryAttack();
    }

    private void TickVanishing()
    {
        StopMoving();
        stateTimer -= Time.fixedDeltaTime;

        SetVisuals(IsFlashVisible());

        if (stateTimer <= 0f)
            EnterHidden();
    }

    private void BeginSpawnReveal()
    {
        state = SpiderState.SpawnReveal;
        stateTimer = spawnVisibleDuration;
        stationaryTimer = 0f;

        SetVisuals(true);
        SetBodyCollider(true);
    }

    private void EnterHidden()
    {
        state = SpiderState.Hidden;
        stationaryTimer = 0f;

        SetVisuals(false);
        SetBodyCollider(false);
        StopMoving();
    }

    private void BeginTelegraph()
    {
        state = SpiderState.Telegraphing;
        stateTimer = telegraphDuration;
        stationaryTimer = 0f;

        transform.position = FindAmbushPosition();

        SetBodyCollider(false);
        SetVisuals(true);
        StopMoving();
    }

    private void BecomeActive()
    {
        state = SpiderState.Active;
        attackReadyTime =
            Time.time + attackAfterEmergingDelay;

        SetVisuals(true);
        SetBodyCollider(true);
    }

    private void BeginVanishing()
    {
        if (state == SpiderState.Hidden ||
            state == SpiderState.Vanishing)
        {
            return;
        }

        state = SpiderState.Vanishing;
        stateTimer = vanishDuration;

        SetBodyCollider(false);
        StopMoving();
    }

    private void TryAttack()
    {
        if (Time.time < attackReadyTime ||
            Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + AttackCooldown;

        DamageTarget(
            ContactDamage,
            target.position);
    }

    private Vector2 FindAmbushPosition()
    {
        for (int attempt = 0; attempt < 12; attempt++)
        {
            Vector2 direction =
                Random.insideUnitCircle.normalized;

            if (direction == Vector2.zero)
                direction = Vector2.right;

            Vector2 point =
                (Vector2)target.position +
                direction * emergeDistance;

            if (roomBounds != null)
                point = roomBounds.ClampPoint(point);

            bool blocked = Physics2D.OverlapCircle(
                point,
                spawnClearance,
                blockedLayer);

            if (!blocked)
                return point;
        }

        Vector2 fallback =
            (Vector2)target.position +
            Vector2.right * emergeDistance;

        return roomBounds != null
            ? roomBounds.ClampPoint(fallback)
            : fallback;
    }

    private bool IsPlayerMoving()
    {
        return playerMovement != null &&
               playerMovement.IsMoving;
    }

    private bool IsFlashVisible()
    {
        if (flashInterval <= 0f)
            return true;

        return Mathf.FloorToInt(
            Time.time / flashInterval) % 2 == 0;
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
