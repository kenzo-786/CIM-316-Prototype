using UnityEngine;
using System.Collections;

public class SpiderAmbushEnemy : EnemyBase
{
    [Header("Ambush")]
    [SerializeField] private float stationaryDelay = 0.6f;
    [SerializeField] private float emergeDistance = 2f;
    [SerializeField] private float attackAfterEmergingDelay = 0.25f;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private SpriteRenderer[] visuals;

    private PlayerMovement playerMovement;
    private bool hidden = true;
    private float stationaryTimer;
    private float attackReadyTime;
    private float nextAttackTime;

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);

        playerMovement = playerTarget != null ? playerTarget.GetComponent<PlayerMovement>() : null;
        SetHidden(true);
    }

    protected override void TickEnemy()
    {
        if (target == null)
            return;

        bool playerMoving = playerMovement != null && playerMovement.IsMoving;

        if (hidden)
        {
            StopMoving();

            if (playerMoving)
            {
                stationaryTimer = 0f;
                return;
            }

            stationaryTimer += Time.fixedDeltaTime;

            if (stationaryTimer >= stationaryDelay)
                EmergeNearPlayer();

            return;
        }

        if (playerMoving)
        {
            SetHidden(true);
            stationaryTimer = 0f;
            return;
        }

        if (!IsTargetInRange(AttackRange))
            MoveToward(target.position);
        else
            TryAttack();
    }

    private void EmergeNearPlayer()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (randomDirection == Vector2.zero)
            randomDirection = Vector2.right;

        transform.position = (Vector2)target.position + randomDirection * emergeDistance;
        SetHidden(false);
        attackReadyTime = Time.time + attackAfterEmergingDelay;
    }

    private void TryAttack()
    {
        StopMoving();

        if (Time.time < attackReadyTime || Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + AttackCooldown;
        DamageTarget(ContactDamage, target.position);
    }

    private void SetHidden(bool value)
    {
        hidden = value;

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (bodyCollider != null)
            bodyCollider.enabled = !hidden;

        if (visuals == null || visuals.Length == 0)
            visuals = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer visual in visuals)
        {
            if (visual != null)
                visual.enabled = !hidden;
        }
    }
}
