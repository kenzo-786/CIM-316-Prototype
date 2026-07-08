using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyProjectileShooter))]
public class ShooterEnemy : EnemyBase
{
    [Header("Shooter")]
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float tooCloseRange = 3f;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float relocateDelay = 0.45f;
    [SerializeField] private float fadeScale = 0.1f;

    private EnemyProjectileShooter shooter;
    private RoomBounds roomBounds;
    private SpriteRenderer[] renderers;
    private float nextShootTime;
    private bool relocating;

    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<EnemyProjectileShooter>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);
        roomBounds = FindObjectOfType<RoomBounds>();
    }

    private void FixedUpdate()
    {
        if (target == null || enemyData == null || IsDead || relocating) return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= tooCloseRange)
        {
            StartCoroutine(RelocateRoutine());
            return;
        }

        if (distance > preferredRange)
        {
            Vector2 direction = target.position - transform.position;
            rb.linearVelocity = direction.normalized * enemyData.moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time < nextShootTime) return;

        nextShootTime = Time.time + shootCooldown;
        shooter.ShootAt(target.position);
    }

    private IEnumerator RelocateRoutine()
    {
        relocating = true;
        rb.linearVelocity = Vector2.zero;

        SetVisible(false);
        transform.localScale = Vector3.one * fadeScale;

        yield return new WaitForSeconds(relocateDelay);

        if (roomBounds != null)
            transform.position = roomBounds.GetRandomPoint();

        transform.localScale = Vector3.one;
        SetVisible(true);

        nextShootTime = Time.time + 0.4f;
        relocating = false;
    }

    private void SetVisible(bool visible)
    {
        foreach (SpriteRenderer renderer in renderers)
            renderer.enabled = visible;
    }
}
