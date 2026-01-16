using UnityEngine;

public class Vyron : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 10f;
        fireRate = 0.8f;
        range = 4f;

    }
    protected override void Shoot()
    {
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
        float actualDamage = damage;

        // 10% chance to land a critical hit - 10 * the damage
        int DiceNumber = Random.Range(0,11);
        if (DiceNumber == 10)
            actualDamage = damage*10;

        Projectile p = projectile.GetComponent<Projectile>();
        if (p != null)
        {
            p.SetDamage(actualDamage);
            p.SetTarget(target);
        }
    }
    
    
}
