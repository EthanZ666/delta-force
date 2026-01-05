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
    [SerializeField] private Transform firePoint;

    protected Transform target;
    
<<<<<<< Updated upstream

<<<<<<< Updated upstream
    protected virtual void Start()
    {
        fireCooldown = 1f / fireRate;
    }
=======
>>>>>>> Stashed changes

   protected virtual void Update()
{
    fireCooldown -= Time.deltaTime;

    FindTarget();

    if (target != null && fireCooldown <= 0f)
    {
<<<<<<< Updated upstream
        if (!projectilePrefab || !firePoint) return;

        fireCooldown -= Time.deltaTime;

        FindTarget();

        if (target && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
=======
   protected virtual void Update()
{
    fireCooldown -= Time.deltaTime;
=======
        Shoot();
        fireCooldown = 1f / fireRate;
    }
}

>>>>>>> Stashed changes

    FindTarget();

    if (target != null && fireCooldown <= 0f)
    {
<<<<<<< Updated upstream
        Shoot();
        fireCooldown = 1f / fireRate;
>>>>>>> Stashed changes
    }
}


    protected void Shoot()
    {
=======
>>>>>>> Stashed changes
        Debug.Log("SHOOT CALLED");
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Projectile p = projectile.GetComponent<Projectile>();
        if (p != null)
        {
            p.SetDamage(damage);
            p.SetTarget(target);
        }
    }

    protected void FindTarget()
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    float shortestDistance = Mathf.Infinity;
    GameObject nearestEnemy = null;
<<<<<<< Updated upstream

    foreach (GameObject enemyObj in enemies)
    {
<<<<<<< Updated upstream
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (GameObject enemyObj in enemies)
        {
            if (!enemyObj) continue;

            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemyObj.transform.position);
            if (distance <= range && distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemyObj.transform;
            }
        }

        target = nearestEnemy;
=======
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy == null || enemy.IsDead) continue;

        float distance = Vector3.Distance(transform.position, enemyObj.transform.position);
        if (distance < shortestDistance && distance <= range)
        {
            shortestDistance = distance;
            nearestEnemy = enemyObj;
        }
>>>>>>> Stashed changes
=======

    foreach (GameObject enemyObj in enemies)
    {
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy == null || enemy.IsDead) continue;

        float distance = Vector3.Distance(transform.position, enemyObj.transform.position);
        if (distance < shortestDistance && distance <= range)
        {
            shortestDistance = distance;
            nearestEnemy = enemyObj;
        }
>>>>>>> Stashed changes
    }

    target = nearestEnemy != null ? nearestEnemy.transform : null;
}
}
