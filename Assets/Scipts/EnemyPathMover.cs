using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class EnemyPathMover : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private string pathPointTag = "PathPoint";
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Base")]
    [SerializeField] private string baseTag = "MilitaryBase";

    private EnemyBase enemy;
    private Transform[] points;
    private int index;

    //private PlayerBaseHealth baseHealth;
    private bool finished;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        CachePathPoints();
        CacheBase();
    }

    private void Update()
    {
        if (finished) return;
        if (enemy.IsDead) return;
        if (points == null || points.Length == 0) return;

        MoveAlongPath();
    }

    private void CachePathPoints()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(pathPointTag);

        if (objs == null || objs.Length == 0)
        {
            Debug.LogError($"No path points found with tag '{pathPointTag}'.");
            points = null;
            return;
        }

        System.Array.Sort(objs, (a, b) => string.CompareOrdinal(a.name, b.name));

        points = new Transform[objs.Length];
        for (int i = 0; i < objs.Length; i++)
            points[i] = objs[i].transform;

        index = 0;
    }

    private void CacheBase()
    {
        GameObject baseObj = GameObject.FindGameObjectWithTag(baseTag);
        if (baseObj == null)
        {
            Debug.LogError($"No base found with tag '{baseTag}'.");
            return;
        }

        // baseHealth = baseObj.GetComponent<PlayerBaseHealth>();
        // if (baseHealth == null)
        // {
        //     Debug.LogError("MilitaryBase object is missing PlayerBaseHealth component.");
        // }
    }

    private void MoveAlongPath()
    {
        Transform target = points[index];

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, target.position, enemy.Speed * Time.deltaTime);
        transform.position = next;

        if (Vector2.Distance(next, target.position) <= arriveDistance)
        {
            bool isLastPoint = (index >= points.Length - 1);

            if (isLastPoint)
            {
                ReachEnd();
                return;
            }

            index++;
        }
    }

    private void ReachEnd()
    {
        finished = true;

        // if (baseHealth != null)
        //     baseHealth.TakeDamage(enemy.Damage);

        Destroy(gameObject);
    }
}
