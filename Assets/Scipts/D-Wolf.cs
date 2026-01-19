using UnityEngine;

public class DWolf : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 5.5f;
        fireRate = 1.5f;
        range = 5f;
        
    }
     protected override void Shoot()
{
    int bulletCount = 1;
    float DiceNumber = Random.Range(1,11);

    // 10% chance to fire 3 bullets
    if (DiceNumber == 10)
    {
        bulletCount = 3;
    }

    for (int i = 0; i < bulletCount; i++)
    {
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
}

}
