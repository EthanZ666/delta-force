using UnityEngine;

public class RegeneratingEnemy : EnemyBase
{
    [Header("Regeneration")]
    [SerializeField] private float regenPerSecond = 5f;
    [SerializeField] private float regenDelayAfterHit = 2f;

    private float lastHitTime;

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
        if (Time.time < lastHitTime + regenDelayAfterHit) return;

        Heal(regenPerSecond * Time.deltaTime);
    }
}
