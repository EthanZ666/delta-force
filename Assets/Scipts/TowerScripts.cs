using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public virtual float damage { get; protected set; } = 10f;
    public virtual float fireRate { get; protected set; } = 1f;
    public virtual float range { get; protected set; } = 5f;

    protected float fireCooldown;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    // Aggregation that attaches an object of the Projectile Class - able to live
    // With out this tower class so it is aggregation
    [SerializeField] private Transform firePoint;

    protected Transform target;

    protected virtual void Start()
    {
        fireCooldown = 1f / fireRate;
    }

    protected virtual void Update()
    {
        if (!projectilePrefab || !firePoint) return;

        fireCooldown -= Time.deltaTime;

        FindTarget();

        if (target && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }

    protected void Shoot()
    {
        // Spawning copies of the aggregated object
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Assigning damage and target for the copies, using methods of
        // the aggregated class
        Projectile p = projectile.GetComponent<Projectile>();
        if (p != null)
        {
            p.SetDamage(damage);
            p.SetTarget(target);
        }
    }
    float CalculateDangerValue(EnemyBase enemy, float distance)
{
    float Dangervalue =
        enemy.Damage * 2.3f +
        // higher damage -> higher value
        enemy.Speed * 3f + 
        // Faster ones gets higher value
        (range - distance); // closer enemies get higher value

    return Dangervalue;
}


    protected void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float MaxDangerValue = Mathf.Infinity;
        Transform targetEnemy = null;

        foreach (GameObject enemyObj in enemies)
        {
            if (!enemyObj) continue;

            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemyObj.transform.position);
            float dangervalue = CalculateDangerValue(enemy, distance);
            if (distance <= range && dangervalue > MaxDangerValue)
            {
                MaxDangerValue = dangervalue;
                targetEnemy = enemyObj.transform;
            }
        }

        target = targetEnemy;
    }
}
