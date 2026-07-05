using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData enemyData;

    protected Rigidbody2D rb;
    protected Health health;
    protected Transform target;

    public event Action<EnemyBase> OnEnemyDied;

    public EnemyData EnemyData => enemyData;
    public bool IsDead => health != null && health.IsDead;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    protected virtual void OnEnable()
    {
        health.OnDied += HandleDied;
    }

    protected virtual void OnDisable()
    {
        health.OnDied -= HandleDied;
    }

    public virtual void Initialize(EnemyData data, Transform playerTarget)
    {
        enemyData = data;
        target = playerTarget;

        if (enemyData != null)
        {
            health.SetMaxHealth(enemyData.maxHealth, true);
        }
    }

    protected virtual void HandleDied()
    {
        rb.linearVelocity = Vector2.zero;
        OnEnemyDied?.Invoke(this);

        EnemyEvents.RaiseEnemyDied(this);
        Destroy(gameObject);
    }
}
