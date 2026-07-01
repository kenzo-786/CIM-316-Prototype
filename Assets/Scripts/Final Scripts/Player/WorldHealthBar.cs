using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private bool hideWhenFull = false;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        health.OnHealthChanged += UpdateBar;
        health.OnDied += Hide;
        UpdateBar(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= UpdateBar;
        health.OnDied -= Hide;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) transform.rotation = mainCamera.transform.rotation;
    }

    private void UpdateBar(float current, float max)
    {
        fillImage.fillAmount = current / max;

        if (visualRoot != null)
            visualRoot.SetActive(!hideWhenFull || current < max);
    }

    private void Hide()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);
    }
}
