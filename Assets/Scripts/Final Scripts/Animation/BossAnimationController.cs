using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Transform idleVisual;
    [SerializeField] private float idleBobHeight = 0.06f;
    [SerializeField] private float idleBobSpeed = 2.2f;

    private Animator animator;
    private readonly HashSet<string> parameterNames = new HashSet<string>();
    private Vector3 idleStartPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (health == null)
            health = GetComponentInParent<Health>();

        if (idleVisual == null)
            idleVisual = transform;

        idleStartPosition = idleVisual.localPosition;

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

        if (idleVisual != null)
            idleVisual.localPosition = idleStartPosition;
    }

    private void LateUpdate()
    {
        if (idleVisual == null || animator == null)
            return;

        bool isIdle = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");

        float offset = isIdle
            ? Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight
            : 0f;

        idleVisual.localPosition =
            idleStartPosition + Vector3.up * offset;
    }

    public void PlayWebFan()
    {
        SetTrigger("WebFan");
    }

    public void PlayVenomCircles()
    {
        SetTrigger("VenomCircles");
    }

    public void PlayWebTraps()
    {
        SetTrigger("WebTraps");
    }

    public void PlaySummon()
    {
        SetTrigger("Summon");
    }

    public void PlayDeath()
    {
        SetTrigger("Death");
    }

    private void SetTrigger(string parameterName)
    {
        if (parameterNames.Contains(parameterName))
            animator.SetTrigger(parameterName);
    }
}
