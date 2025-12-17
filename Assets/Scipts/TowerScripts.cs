
using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public virtual float damage { get; protected set; }= 10f;
    public virtual float fireRate { get; protected set; }= 1f;   
    public virtual float range { get; protected set; }= 5f;

    protected float fireCooldown = 0f;

    protected Transform target;

    protected virtual void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (target != null && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }

    protected abstract void Shoot();
    // void Shoot()
    // {
    //     GameObject projectile = Instantiate(
    //         projectilePrefab,
    //         firePoint.position,
    //         firePoint.rotation
    //     );

    //     Projectile p = projectile.GetComponent<Projectile>();
    //     p.damage = damage;
    //     p.SetTarget(target);
    // }

    protected void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        target = nearestEnemy != null ? nearestEnemy.transform : null;
    }
}


