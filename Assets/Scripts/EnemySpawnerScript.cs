using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    public float spawnDistanceOffset = 2f;

    private float nextSpawnTime;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || mainCam == null) return;

        float screenHeight = 2f * mainCam.orthographicSize;
        float screenWidth = screenHeight * mainCam.aspect;

        int side = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        float halfWidth = screenWidth / 2f;
        float halfHeight = screenHeight / 2f;

        switch (side)
        {
            case 0: // Top
                spawnPos = new Vector3(Random.Range(-halfWidth, halfWidth), halfHeight + spawnDistanceOffset, 0);
                break;
            case 1: // Bottom
                spawnPos = new Vector3(Random.Range(-halfWidth, halfWidth), -halfHeight - spawnDistanceOffset, 0);
                break;
            case 2: // Left
                spawnPos = new Vector3(-halfWidth - spawnDistanceOffset, Random.Range(-halfHeight, halfHeight), 0);
                break;
            case 3: // Right
                spawnPos = new Vector3(halfWidth + spawnDistanceOffset, Random.Range(-halfHeight, halfHeight), 0);
                break;
        }

        spawnPos += mainCam.transform.position;
        spawnPos.z = 0;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
