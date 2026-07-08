using UnityEngine;
using System.Collections;

public class TankEnemy : EnemyBase
{
    [Header("Tank Jump")]
    [SerializeField] private float jumpWindup = 0.45f;
    [SerializeField] private float jumpDuration = 0.55f;
    [SerializeField] private float jumpCooldown = 1.2f;
    [SerializeField] private float landingRadius = 1.4f;
    [SerializeField] private float landingDamage = 25f;
    [SerializeField] private float playerJumpChance = 0.45f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float jumpScale = 1.35f;

    private RoomBounds roomBounds;
    private bool jumping;

    public override void Initialize(EnemyData data, Transform playerTarget)
    {
        base.Initialize(data, playerTarget);
        roomBounds = FindObjectOfType<RoomBounds>();
        StartCoroutine(JumpLoop());
    }

    private IEnumerator JumpLoop()
    {
        yield return new WaitForSeconds(0.8f);

        while (!IsDead)
        {
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(jumpWindup);

            Vector3 targetPosition = PickLandingPoint();
            yield return JumpTo(targetPosition);

            DamageLandingArea();

            yield return new WaitForSeconds(jumpCooldown);
        }
    }

    private Vector3 PickLandingPoint()
    {
        bool jumpAtPlayer = target != null && Random.value <= playerJumpChance;

        if (jumpAtPlayer)
            return target.position;

        if (roomBounds != null)
            return roomBounds.GetRandomPoint();

        return transform.position;
    }

    private IEnumerator JumpTo(Vector3 destination)
    {
        jumping = true;

        Vector3 start = transform.position;
        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;

            Vector3 flatPosition = Vector3.Lerp(start, destination, t);

            if (visualRoot != null)
            {
                float arc = Mathf.Sin(t * Mathf.PI);
                visualRoot.localScale = Vector3.one * Mathf.Lerp(1f, jumpScale, arc);
            }

            transform.position = flatPosition;
            yield return null;
        }

        transform.position = destination;

        if (visualRoot != null)
            visualRoot.localScale = Vector3.one;

        jumping = false;
    }

    private void DamageLandingArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            landingRadius,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(new DamageInfo(
                    landingDamage,
                    gameObject,
                    hit.transform.position
                ));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, landingRadius);
    }
}
