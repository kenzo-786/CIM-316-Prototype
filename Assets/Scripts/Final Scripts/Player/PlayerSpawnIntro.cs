using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerSpawnIntro : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private float startHeight = 8f;
    [SerializeField] private float fallDuration = 0.55f;
    [SerializeField] private GameObject impactEffectPrefab;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        Vector3 landingPosition = player.position;
        Vector3 startPosition = landingPosition + Vector3.up * startHeight;

        player.position = startPosition;

        if (movement != null)
            movement.enabled = false;

        if (weaponController != null)
            weaponController.enabled = false;

        float timer = 0f;

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fallDuration;
            float eased = t * t;

            player.position = Vector3.Lerp(startPosition, landingPosition, eased);
            yield return null;
        }

        player.position = landingPosition;

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, landingPosition, Quaternion.identity);

        if (movement != null)
            movement.enabled = true;

        if (weaponController != null)
            weaponController.enabled = true;
    }
}
