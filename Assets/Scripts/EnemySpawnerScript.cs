using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    [Header("Spawn Settings")]
    public EnemyData[] enemyTypes;
    public float spawnRate = 2f;
    public float spawnOffset = 3f;

    [Header("Wave Intensity")]
    public int minSpawnCount = 3;
    public int maxSpawnCount = 6;

    [Header("Difficulty Scaling")]
    public float scalingInterval = 15f;
    public float spawnRateReduction = 0.1f;
    public float minSpawnRateLimit = 0.5f;

    public int countIncreaseAmount = 1;

    private float nextSpawnTime;
    private float scalingTimer;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        scalingTimer += Time.deltaTime;
        if (scalingTimer >= scalingInterval)
        {
            scalingTimer = 0;
            IncreaseDifficulty();
        }

        if (Time.time >= nextSpawnTime)
        {
            int amountToSpawn = Random.Range(minSpawnCount, maxSpawnCount + 1);

            for (int i = 0; i < amountToSpawn; i++)
            {
                Spawn();
            }

            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void IncreaseDifficulty()
    {
        spawnRate = Mathf.Max(minSpawnRateLimit, spawnRate - spawnRateReduction);

        maxSpawnCount += countIncreaseAmount;

        if (maxSpawnCount % 2 == 0)
        {
            minSpawnCount++;
        }
    }

    void Spawn()
    {
        if (enemyTypes == null || enemyTypes.Length == 0) return;

        EnemyData randomData = enemyTypes[Random.Range(0, enemyTypes.Length)];
        if (randomData == null || randomData.enemyPrefab == null) return;

        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        int side = Random.Range(0, 4);
        Vector3 pos = cam.transform.position;

        float xVariation = Random.Range(-1f, 1f);
        float yVariation = Random.Range(-1f, 1f);

        if (side == 0) pos += new Vector3(Random.Range(-w / 2, w / 2), h / 2 + spawnOffset + yVariation, 0);
        else if (side == 1) pos += new Vector3(Random.Range(-w / 2, w / 2), -h / 2 - spawnOffset + yVariation, 0);
        else if (side == 2) pos += new Vector3(-w / 2 - spawnOffset + xVariation, Random.Range(-h / 2, h / 2), 0);
        else pos += new Vector3(w / 2 + spawnOffset + xVariation, Random.Range(-h / 2, h / 2), 0);

        pos.z = 0;

        GameObject newEnemy = Instantiate(randomData.enemyPrefab, pos, Quaternion.identity);

        EnemyAI ai = newEnemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.data = randomData;
        }
    }
}
