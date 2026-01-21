using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private string pathPointTag = "PathPoint";

    [Header("Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int spawnCount = 10;

    private Transform spawnPoint;

    private void Awake()
    {
        spawnPoint = FindFirstPathPoint();
    }

    private void Start()
    {
        if (spawnPoint == null) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne()
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
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
