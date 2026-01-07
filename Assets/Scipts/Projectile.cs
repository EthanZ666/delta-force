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
        target = newTarget;
    }

    public void SetDamage(float value)
    {
        damage = value;
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

        // 🔑 THIS IS THE IMPORTANT PART
        if (!target) // catches destroyed targets


        // 🔑 THIS IS THE IMPORTANT PART
        if (!target) // catches destroyed targets

        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (target.position - transform.position);
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
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
