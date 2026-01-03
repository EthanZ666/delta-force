
using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public virtual float damage { get; protected set; }= 10f;
    public virtual float fireRate { get; protected set; }= 1f;   
    public virtual float range { get; protected set; }= 5f;

    protected float fireCooldown = 0f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    protected Transform target;
    

   protected virtual void Update()
{
    fireCooldown -= Time.deltaTime;

    FindTarget();

    if (target != null && fireCooldown <= 0f)
    {
        Shoot();
        fireCooldown = 1f / fireRate;
    }
}


    protected virtual void Shoot()
    {
<<<<<<< Updated upstream
        if (projectilePrefab == null || firePoint == null) 
            return;
=======
        Debug.Log("SHOOT CALLED");
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
>>>>>>> Stashed changes

    GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    Projectile p = projectile.GetComponent<Projectile>();

    if (p == null) 
        return;

    p.SetDamage(damage);
    p.SetTarget(target);
    }


    protected void FindTarget()
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    float shortestDistance = Mathf.Infinity;
    GameObject nearestEnemy = null;

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
    }

    target = nearestEnemy != null ? nearestEnemy.transform : null;
}
}


