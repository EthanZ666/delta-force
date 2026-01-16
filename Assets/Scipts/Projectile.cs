using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private float damage;
    private Transform target;
    private float timer;

    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
            target = newTarget;
        else
            throw new Exception("No target is passed in");
    }

    public void SetDamage(float value)
    {
        if (value >= 0)
            damage = value;
        else
            throw new Exception("Damage less then 0");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (!target) 
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (target.position - transform.position);
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        // Done by GPT for tracing the enemy
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
