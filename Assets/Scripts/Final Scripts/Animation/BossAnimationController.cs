using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    [SerializeField] private Health health;

    private Animator animator;
    private readonly HashSet<string> parameterNames = new HashSet<string>();

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (health == null)
            health = GetComponentInParent<Health>();

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
