using UnityEngine;

public class AutoTargetFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerWeaponController
       weaponController;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private GameObject targetMarkerPrefab;

    [Header("Marker")]
    [SerializeField]
    private Vector3 targetOffset =
        new Vector3(0f, 0.9f, 0f);

    [SerializeField, Min(0f)]
    private float pulseAmount = 0.12f;

    [SerializeField, Min(0f)]
    private float pulseSpeed = 5f;

    [SerializeField]
    private bool rotateMarker = true;

    [SerializeField]
    private float rotationSpeed = 120f;

    private GameObject markerInstance;

    private Vector3 markerBaseScale =
        Vector3.one;

    private void Awake()
    {
        if (weaponController == null)
        {
            weaponController =
                GetComponent
                    <PlayerWeaponController>();
        }

        if (playerMovement == null)
        {
            playerMovement =
                GetComponent<PlayerMovement>();
        }

        if (targetMarkerPrefab != null)
        {
            markerInstance =
                Instantiate(
                    targetMarkerPrefab);

            markerBaseScale =
                markerInstance
                    .transform.localScale;

            markerInstance.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (markerInstance == null ||
            weaponController == null)
        {
            return;
        }

        Transform target =
            weaponController.CurrentTarget;

        bool shouldShow =
            weaponController
                .AutoTargetEnemies &&
            weaponController
                .CombatActive &&
            (playerMovement == null ||
             !playerMovement.IsMoving) &&
            target != null &&
            Time.timeScale > 0f;

        markerInstance.SetActive(
            shouldShow);

        if (!shouldShow)
            return;

        markerInstance.transform.position =
            target.position +
            targetOffset;

        float pulse =
            1f +
            Mathf.Sin(
                Time.unscaledTime *
                pulseSpeed) *
            pulseAmount;

        markerInstance
            .transform.localScale =
            markerBaseScale * pulse;

        if (rotateMarker)
        {
            markerInstance.transform.Rotate(
                0f,
                0f,
                rotationSpeed *
                Time.unscaledDeltaTime);
        }
    }

    private void OnDestroy()
    {
        if (markerInstance != null)
            Destroy(markerInstance);
    }
}
