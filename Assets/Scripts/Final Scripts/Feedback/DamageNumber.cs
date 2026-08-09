using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour , IPoolable
{
    [Header("References")]
    [SerializeField]
    private TMP_Text text;

    [Header("Movement")]
    [SerializeField]
    private float lifetime = 0.65f;

    [SerializeField]
    private float floatSpeed = 1.2f;

    [Header("Colors")]
    [SerializeField]
    private Color normalColor =
        Color.white;

    [SerializeField]
    private Color criticalColor =
        Color.yellow;

    [SerializeField]
    private Color healingColor =
        new Color(
            0.25f,
            1f,
            0.35f,
            1f);

    private Coroutine routine;
    private float baseFontSize;

    private void Awake()
    {
        if (text == null)
        {
            text =
                GetComponentInChildren<TMP_Text>();
        }

        if (text != null)
            baseFontSize = text.fontSize;
    }

    public void Show(
        float amount,
        bool critical)
    {
        string value =
            Mathf.CeilToInt(amount)
                .ToString();

        Color color =
            critical
                ? criticalColor
                : normalColor;

        float sizeMultiplier =
            critical ? 1.15f : 1f;

        ShowText(
            value,
            color,
            sizeMultiplier);
    }

    public void ShowHealing(float amount)
    {
        string value =
            "+" +
            Mathf.CeilToInt(amount);

        ShowText(
            value,
            healingColor,
            1f);
    }

    public void ShowMessage(
        string message,
        Color color)
    {
        ShowText(
            message,
            color,
            1.2f);
    }

    private void ShowText(
        string message,
        Color color,
        float sizeMultiplier)
    {
        if (text != null)
        {
            text.text = message;
            text.color = color;

            text.fontSize =
                baseFontSize *
                sizeMultiplier;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine =
            StartCoroutine(
                FloatRoutine());
    }

    public void OnSpawnedFromPool()
    {
    }

    public void OnReturnedToPool()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator FloatRoutine()
    {
        float timer = 0f;

        while (timer < lifetime)
        {
            timer +=
                Time.unscaledDeltaTime;

            transform.position +=
                Vector3.up *
                floatSpeed *
                Time.unscaledDeltaTime;

            yield return null;
        }

        PooledProjectileUtility.Despawn(
            gameObject);
    }
}
