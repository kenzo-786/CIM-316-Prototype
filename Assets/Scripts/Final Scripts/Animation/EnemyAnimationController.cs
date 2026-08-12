using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D movementBody;
    [SerializeField] private Health health;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField, Min(0f)] private float movingThreshold = 0.05f;

    private Animator animator;
    private readonly HashSet<string> parameterNames = new HashSet<string>();
    private Vector2 lastDirection = Vector2.down;
    private bool useExternalMotion;
    private bool externalIsMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (movementBody == null)
            movementBody = GetComponentInParent<Rigidbody2D>();

        if (health == null)
            health = GetComponentInParent<Health>();

        if (visual == null)
            visual = GetComponent<SpriteRenderer>();

        if (visual == null)
            visual = GetComponentInChildren<SpriteRenderer>();

        foreach (AnimatorControllerParameter parameter in animator.parameters)
            parameterNames.Add(parameter.name);
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDied += PlayDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= PlayDeath;
    }

    private void Update()
    {
        if (useExternalMotion)
        {
            ApplyAnimation(externalIsMoving);
            return;
        }

        if (movementBody == null)
            return;

        Vector2 velocity = movementBody.linearVelocity;
        bool isFollowing = velocity.sqrMagnitude > movingThreshold * movingThreshold;

        if (isFollowing)
            lastDirection = GetCardinalDirection(velocity);

        ApplyAnimation(isFollowing);
    }

    public void SetMovementDirection(Vector2 direction)
    {
        useExternalMotion = true;
        externalIsMoving = direction.sqrMagnitude > movingThreshold * movingThreshold;

        if (externalIsMoving)
            lastDirection = GetCardinalDirection(direction);

        ApplyAnimation(externalIsMoving);
    }

    public void SetStationary()
    {
        useExternalMotion = true;
        externalIsMoving = false;
        ApplyAnimation(false);
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        useExternalMotion = true;
        lastDirection = GetCardinalDirection(direction);
        ApplyAnimation(externalIsMoving);
    }

    public void SetSpinning(bool value)
    {
        SetBool("IsSpinning", value);
    }

    public void UsePhysicsMovement()
    {
        useExternalMotion = false;
    }

    public void PlayAttack()
    {
        SetTrigger("IsAttacking");
    }

    public void PlayHurt()
    {
        SetTrigger("Hurt");
    }

    public void PlayDeath()
    {
        SetTrigger("Death");
    }

    private Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x >= 0f ? Vector2.right : Vector2.left;

        return direction.y >= 0f ? Vector2.up : Vector2.down;
    }

    private void ApplyAnimation(bool isFollowing)
    {
        if (visual != null)
            visual.flipX = false;

        SetBool("IsFollowing", isFollowing);
        SetFloat("XDirection", lastDirection.x);
        SetFloat("YDirection", lastDirection.y);
    }

    private void SetBool(string parameterName, bool value)
    {
        if (parameterNames.Contains(parameterName))
            animator.SetBool(parameterName, value);
    }

    private void SetFloat(string parameterName, float value)
    {
        if (parameterNames.Contains(parameterName))
            animator.SetFloat(parameterName, value);
    }

    private void SetTrigger(string parameterName)
    {
        if (parameterNames.Contains(parameterName))
            animator.SetTrigger(parameterName);
    }
}
