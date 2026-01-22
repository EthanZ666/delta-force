using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private string pathPointTag = "PathPoint";

    [Header("Enemy Prefabs (put easier ones first)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float timeBetweenWaves = 2f;

    [Header("Wave Difficulty Scaling")]
    [SerializeField] private int startingSpawnCount = 8;
    [SerializeField] private int spawnCountIncreasePerWave = 2;

    [SerializeField] private float startingSpawnInterval = 1.0f;
    [SerializeField] private float spawnIntervalDecreasePerWave = 0.05f;
    [SerializeField] private float minimumSpawnInterval = 0.25f;

    [Header("Prefab Difficulty Progression")]
    [Tooltip("How many waves until ALL prefabs are allowed to spawn.")]
    [SerializeField] private int wavesToUnlockAllPrefabs = 10;

    private Transform spawnPoint;

    private void Awake()
    {
        spawnPoint = FindFirstPathPoint();
    }

    private void Start()
    {
        if (spawnPoint == null) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        StartCoroutine(EndlessSpawnRoutine());
    }

    private IEnumerator EndlessSpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        int wave = 1;

        while (true)
        {
            int spawnCount = startingSpawnCount + (wave - 1) * spawnCountIncreasePerWave;

            float interval = startingSpawnInterval - (wave - 1) * spawnIntervalDecreasePerWave;
            interval = Mathf.Max(minimumSpawnInterval, interval);

            int unlockedCount = GetUnlockedPrefabCount(wave);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnOne(unlockedCount);
                yield return new WaitForSeconds(interval);
            }

            wave++;
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnOne(int unlockedPrefabCount)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        unlockedPrefabCount = Mathf.Clamp(unlockedPrefabCount, 1, enemyPrefabs.Length);

        GameObject prefab = enemyPrefabs[Random.Range(0, unlockedPrefabCount)];
        if (prefab == null) return;

        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }

    private int GetUnlockedPrefabCount(int wave)
    {
        if (enemyPrefabs.Length <= 1) return enemyPrefabs.Length;

        float t = Mathf.InverseLerp(1f, Mathf.Max(2f, wavesToUnlockAllPrefabs), wave);
        int count = 1 + Mathf.FloorToInt(t * (enemyPrefabs.Length - 1));

        return Mathf.Clamp(count, 1, enemyPrefabs.Length);
    }

    private Transform FindFirstPathPoint()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(pathPointTag);

        if (objs == null || objs.Length == 0)
        {
            Debug.LogError($"EnemySpawner: No path points found with tag '{pathPointTag}'.");
            return null;
        }

        System.Array.Sort(objs, (a, b) => string.CompareOrdinal(a.name, b.name));
        return objs[0].transform;
    }
}
