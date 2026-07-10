using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private SpriteRenderer visual;

    protected Rigidbody2D rb;
    protected Health health;
    protected Transform target;

    private bool isDead;
    private RoomDifficultySnapshot currentDifficulty = RoomDifficultySnapshot.Default;

    public event Action<EnemyBase> OnEnemyDied;

    public EnemyData EnemyData => enemyData;
    public Transform Target => target;
    public RoomDifficultySnapshot CurrentDifficulty => currentDifficulty;
    public bool IsDead => isDead;

    protected float MoveSpeed => enemyData != null ? enemyData.moveSpeed * currentDifficulty.moveSpeedMultiplier : 0f;
    protected float ContactDamage => enemyData != null ? enemyData.contactDamage * currentDifficulty.damageMultiplier : 0f;
    protected float AttackRange => enemyData != null ? enemyData.attackRange : 1f;
    protected float AttackCooldown => enemyData != null ? enemyData.attackCooldown : 1f;
    protected SpriteRenderer Visual => visual;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (visual == null)
            visual = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void OnEnable()
    {
        isDead = false;

        if (health != null)
            health.OnDied += HandleDied;
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    public virtual void Initialize(EnemyData data, Transform playerTarget)
    {
        enemyData = data;
        target = playerTarget;
        isDead = false;

        if (enemyData != null && health != null)
            health.SetMaxHealth(enemyData.maxHealth, true);
    }

    public virtual void ApplyDifficulty(RoomDifficultySnapshot difficulty)
    {
        currentDifficulty = difficulty;

        if (enemyData != null && health != null)
            health.SetMaxHealth(enemyData.maxHealth * currentDifficulty.healthMultiplier, true);
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || target == null || enemyData == null)
            return;

        TickEnemy();
    }

    protected abstract void TickEnemy();

    protected void MoveToward(Vector2 worldPosition)
    {
        Vector2 direction = (worldPosition - rb.position).normalized;
        rb.MovePosition(rb.position + direction * MoveSpeed * Time.fixedDeltaTime);
        FaceDirection(direction);
    }

    protected void MoveAwayFrom(Vector2 worldPosition)
    {
        Vector2 direction = (rb.position - worldPosition).normalized;
        rb.MovePosition(rb.position + direction * MoveSpeed * Time.fixedDeltaTime);
        FaceDirection(direction);
    }

    protected void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected bool IsTargetInRange(float range)
    {
        if (target == null)
            return false;

        return Vector2.Distance(transform.position, target.position) <= range;
    }

    protected void DamageTarget(float damage, Vector2 hitPoint)
    {
        if (target == null || damage <= 0f)
            return;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(new DamageInfo(damage, gameObject, hitPoint));
    }

    protected IDamageable GetDamageable(Collider2D hit)
    {
        if (hit == null)
            return null;

        IDamageable damageable = hit.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = hit.GetComponentInParent<IDamageable>();

        return damageable;
    }

    protected void FaceDirection(Vector2 direction)
    {
        if (visual == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        visual.flipX = direction.x < 0f;
    }

    private void HandleDied()
    {
        if (isDead)
            return;

        isDead = true;
        OnDeathStarted();
        OnEnemyDied?.Invoke(this);
        DestroyEnemyObject();
    }

    protected virtual void OnDeathStarted()
    {
    }

    protected virtual void DestroyEnemyObject()
    {
        Destroy(gameObject);
    }
}
