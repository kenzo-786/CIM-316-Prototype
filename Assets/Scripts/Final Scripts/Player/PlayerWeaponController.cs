using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private PlayerWeaponBase startingWeapon;

    [Header("Firing Rules")]
    [SerializeField] private bool autoFireWhenStationary = true;
    [SerializeField] private bool allowAttackWhileMoving;
    [SerializeField] private bool autoTargetEnemies;

    [Header("Combat State")]
    [SerializeField] private bool combatActive = true;

    [Header("Auto Targeting")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] private float targetRange = 25f;
    [SerializeField] private float targetRefreshInterval = 0.1f;
    [SerializeField] private bool requireLineOfSight = true;

    private PlayerMovement movement;
    private PlayerWeaponBase currentWeapon;
    private Transform currentTarget;
    private float nextTargetRefreshTime;

    public PlayerWeaponBase CurrentWeapon => currentWeapon;
    public Transform CurrentTarget => currentTarget;
    public bool AutoTargetEnemies => autoTargetEnemies;
    public bool CombatActive => combatActive;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        currentWeapon = startingWeapon;
    }

    private void Update()
    {
        if (!combatActive ||
            currentWeapon == null ||
            movement == null ||
            Time.timeScale <= 0f)
        {
            return;
        }

        if (movement.IsMoving &&
            !allowAttackWhileMoving)
        {
            return;
        }

        Vector2 attackDirection =
            movement.AimDirection;

        if (autoTargetEnemies)
        {
            RefreshTargetWhenNeeded();

            if (currentTarget == null)
            {
                currentWeapon.SetAttackTarget(null);
                return;
            }

            attackDirection =
                ((Vector2)currentTarget.position -
                 (Vector2)transform.position).normalized;

            currentWeapon.SetAttackTarget(currentTarget);
        }
        else
        {
            currentTarget = null;
            currentWeapon.SetAttackTarget(null);
        }

        bool wantsToAttack =
            autoTargetEnemies ||
            autoFireWhenStationary ||
            Input.GetMouseButton(0);

        if (wantsToAttack)
            currentWeapon.TryAttack(attackDirection);
    }

    public void EquipWeapon(PlayerWeaponBase weapon)
    {
        if (weapon == null)
            return;

        currentWeapon = weapon;
        currentWeapon.SetAttackTarget(currentTarget);
    }

    public void SetCombatActive(bool value)
    {
        combatActive = value;

        if (!combatActive)
        {
            currentTarget = null;
            nextTargetRefreshTime = 0f;

            if (currentWeapon != null)
                currentWeapon.SetAttackTarget(null);
        }
    }

    public void SetAllowAttackWhileMoving(bool value)
    {
        allowAttackWhileMoving = value;
    }

    public void SetAutoFireWhenStationary(bool value)
    {
        autoFireWhenStationary = value;
    }

    public void SetAutoTargeting(bool value)
    {
        autoTargetEnemies = value;
        currentTarget = null;
        nextTargetRefreshTime = 0f;

        if (currentWeapon != null)
            currentWeapon.SetAttackTarget(null);
    }

    private void RefreshTargetWhenNeeded()
    {
        bool targetStillValid =
            IsTargetValid(currentTarget);

        if (targetStillValid &&
            Time.time < nextTargetRefreshTime)
        {
            return;
        }

        currentTarget = FindNearestVisibleEnemy();

        nextTargetRefreshTime =
            Time.time +
            Mathf.Max(0.02f, targetRefreshInterval);
    }

    private Transform FindNearestVisibleEnemy()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                targetRange,
                enemyLayer
            );

        EnemyBase closestEnemy = null;
        float closestDistanceSquared =
            float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy =
                hit.GetComponentInParent<EnemyBase>();

            if (enemy == null ||
                enemy.IsDead ||
                !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!HasLineOfSight(enemy.transform))
                continue;

            float distanceSquared =
                ((Vector2)enemy.transform.position -
                 (Vector2)transform.position)
                .sqrMagnitude;

            if (distanceSquared >=
                closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared =
                distanceSquared;

            closestEnemy = enemy;
        }

        return closestEnemy != null
            ? closestEnemy.transform
            : null;
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null ||
            !target.gameObject.activeInHierarchy)
        {
            return false;
        }

        EnemyBase enemy =
            target.GetComponentInParent<EnemyBase>();

        if (enemy == null || enemy.IsDead)
            return false;

        float distanceSquared =
            ((Vector2)target.position -
             (Vector2)transform.position)
            .sqrMagnitude;

        if (distanceSquared >
            targetRange * targetRange)
        {
            return false;
        }

        return HasLineOfSight(target);
    }

    private bool HasLineOfSight(Transform target)
    {
        if (!requireLineOfSight ||
            obstructionLayer.value == 0)
        {
            return true;
        }

        RaycastHit2D obstruction =
            Physics2D.Linecast(
                transform.position,
                target.position,
                obstructionLayer
            );

        return obstruction.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            targetRange
        );
    }
}
