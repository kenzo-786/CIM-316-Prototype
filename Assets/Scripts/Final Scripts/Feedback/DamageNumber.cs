using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour , IPoolable
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float lifetime = 0.65f;
    [SerializeField] private float floatSpeed = 1.2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;

    private Coroutine routine;
    private float baseFontSize;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();

        if (text != null)
            baseFontSize = text.fontSize;
    }

    public void Show(float amount, bool critical)
    {
        if (text != null)
        {
            text.text = Mathf.CeilToInt(amount).ToString();
            text.color = critical ? criticalColor : normalColor;
            text.fontSize = critical ? baseFontSize * 1.15f : baseFontSize;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FloatRoutine());
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
            timer += Time.unscaledDeltaTime;
            transform.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;
            yield return null;
        }

        PooledProjectileUtility.Despawn(gameObject);
    }
}
