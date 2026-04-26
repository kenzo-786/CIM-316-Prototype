using UnityEngine;
using System.Collections;


public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    [Header("Enemy Data")]
    public EnemyData[] enemyTypes;
    public float spawnOffset = 5f;

    [Header("Wave Configuration")]
    public int currentWave = 0;
    public int baseEnemies = 5;
    public float timeBetweenWaves = 3f;

    [SerializeField] private int enemiesRemainingToSpawn;
    [SerializeField] private int enemiesCurrentlyAlive;

    private Camera cam;
    private bool isSpawning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        cam = Camera.main;
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (isSpawning) return;
        currentWave++;
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;
        enemiesRemainingToSpawn = baseEnemies + (currentWave * 2);

        Debug.Log("Wave " + currentWave + " starting. Enemies to spawn: " + enemiesRemainingToSpawn);

        while (enemiesRemainingToSpawn > 0)
        {
            SpawnEnemy();
            enemiesRemainingToSpawn--;
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
    }

    void SpawnEnemy()
    {
        if (enemyTypes == null || enemyTypes.Length == 0) return;

        EnemyData data = enemyTypes[Random.Range(0, enemyTypes.Length)];
        if (data.enemyPrefab == null) return;

        
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        int side = Random.Range(0, 4);
        Vector3 pos = cam.transform.position;

        if (side == 0) pos += new Vector3(Random.Range(-w / 2, w / 2), h / 2 + spawnOffset, 0);
        else if (side == 1) pos += new Vector3(Random.Range(-w / 2, w / 2), -h / 2 - spawnOffset, 0);
        else if (side == 2) pos += new Vector3(-w / 2 - spawnOffset, Random.Range(-h / 2, h / 2), 0);
        else pos += new Vector3(w / 2 + spawnOffset, Random.Range(-h / 2, h / 2), 0);

        pos.z = 0;

        GameObject enemy = Instantiate(data.enemyPrefab, pos, Quaternion.identity);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.data = data;

        enemiesCurrentlyAlive++;
    }

    public void OnEnemyDeath()
    {
        enemiesCurrentlyAlive--;

        
        if (enemiesCurrentlyAlive <= 0 && enemiesRemainingToSpawn <= 0 && !isSpawning)
        {
            Invoke("StartNextWave", timeBetweenWaves);
        }
    }

}
