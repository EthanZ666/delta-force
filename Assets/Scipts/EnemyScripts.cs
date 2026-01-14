using UnityEngine;
using System;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Enemy Damage")]
    [SerializeField] protected float damage = 10f;

    public virtual float Damage => damage;

    [Header("Targeting")]
    public virtual bool IsCamouflaged => false;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<EnemyBase> Died;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
    }

    protected virtual void Update()
    {
        if (IsDead) return;
        AbilityUpdate();
    }

    protected virtual void AbilityUpdate() { }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        float finalDamage = Mathf.Max(0f, ModifyIncomingDamage(amount));
        if (finalDamage <= 0f) return;

        CurrentHealth -= finalDamage;
        OnDamaged(finalDamage);

        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
        }
    }

    protected virtual float ModifyIncomingDamage(float amount) => amount;
    protected virtual void OnDamaged(float damageTaken) { }

    protected void Heal(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Died?.Invoke(this);
        Destroy(gameObject);
    }

    public bool IsTargetableByTower(bool towerHasCamoDetection)
    {
        return !IsCamouflaged || towerHasCamoDetection;
    }
}
