using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimScript : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 velocity = rb.linearVelocity;

        // Check if the enemy is currently moving
        bool isFollowing = velocity.sqrMagnitude > 0.01f;

        // Remember the last direction the enemy was moving
        if (isFollowing)
        {
            lastDirection = velocity.normalized;
        }

        // Walking / following
        animator.SetBool("IsFollowing", isFollowing);

        // Direction for Blend Tree
        animator.SetFloat("XDirection", lastDirection.x);
        animator.SetFloat("YDirection", lastDirection.y);
    }

    public void PlayAttack()
    {
        animator.SetTrigger("IsAttacking");
    }
}