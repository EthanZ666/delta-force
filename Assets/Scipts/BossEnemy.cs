using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("Boss Regeneration")]
    [SerializeField] private float regenPerSecond = 8f;
    [SerializeField] private float regenDelayAfterHit = 1.5f;

    [Header("Boss Acceleration")]
    [SerializeField] private float accelerationPerSecond = 0.15f;
    [SerializeField] private float maxSpeed = 6f;

    private float lastHitTime;

    public override float Speed => speed;

    protected override void Awake()
    {
        base.Awake();
        lastHitTime = -999f;
    }

    protected override void OnDamaged(float damageTaken)
    {
        base.OnDamaged(damageTaken);
        lastHitTime = Time.time;
    }

    protected override void AbilityUpdate()
    {
        speed = Mathf.Min(maxSpeed, speed + accelerationPerSecond * Time.deltaTime);

        if (Time.time < lastHitTime + regenDelayAfterHit) return;

        Heal(regenPerSecond * Time.deltaTime);
    }
}
