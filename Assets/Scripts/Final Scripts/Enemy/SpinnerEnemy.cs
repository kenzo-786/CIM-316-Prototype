using UnityEngine;
using System.Collections;

public class SpinnerEnemy : EnemyBase
{
    private enum SpinnerState
    {
        Walking,
        Windup,
        Spinning,
        Recovering
    }

    [Header("Spinner")]
    [SerializeField] private float walkDuration = 1.5f;
    [SerializeField] private float windupDuration = 0.4f;
    [SerializeField] private float spinDuration = 1.2f;
    [SerializeField] private float recoverDuration = 0.7f;
    [SerializeField] private float spinSpeedMultiplier = 2.5f;
    [SerializeField] private float spinDamageMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 900f;

    private SpinnerState state;
    private Vector2 spinDirection;
    private float nextAttackTime;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);
        StartCoroutine(StateRoutine());
    }

    private void FixedUpdate()
    {
        if (target == null || enemyData == null || IsDead) return;

        switch (state)
        {
            case SpinnerState.Walking:
                WalkTowardPlayer();
                TryDamagePlayer(enemyData.contactDamage);
                break;

            case SpinnerState.Windup:
                rb.linearVelocity = Vector2.zero;
                break;

            case SpinnerState.Spinning:
                rb.linearVelocity = spinDirection * enemyData.moveSpeed * spinSpeedMultiplier;
                transform.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime);
                TryDamagePlayer(enemyData.contactDamage * spinDamageMultiplier);
                break;

            case SpinnerState.Recovering:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    private IEnumerator StateRoutine()
    {
        while (!IsDead)
        {
            state = SpinnerState.Walking;
            yield return new WaitForSeconds(walkDuration);

            state = SpinnerState.Windup;
            rb.linearVelocity = Vector2.zero;

            if (target != null)
                spinDirection = ((Vector2)target.position - rb.position).normalized;

            yield return new WaitForSeconds(windupDuration);

            state = SpinnerState.Spinning;
            yield return new WaitForSeconds(spinDuration);

            state = SpinnerState.Recovering;
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(recoverDuration);
        }
    }

    private void WalkTowardPlayer()
    {
        Vector2 direction = target.position - transform.position;
        rb.linearVelocity = direction.normalized * enemyData.moveSpeed;
    }

    private void TryDamagePlayer(float damage)
    {
        if (Time.time < nextAttackTime) return;
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > enemyData.attackRange)
            return;

        nextAttackTime = Time.time + enemyData.attackCooldown;

        IDamageable damageable = target.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = target.GetComponentInChildren<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(new DamageInfo(
                damage,
                gameObject,
                target.position
            ));
        }
    }
}
