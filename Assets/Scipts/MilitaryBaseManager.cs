using System;
using UnityEngine;

public sealed class MilitaryBase : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public event Action<float> HealthChanged;
    public event Action Destroyed;

    private void Awake()
    {
        currentHealth = Mathf.Max(0f, maxHealth);
        HealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        if (currentHealth < 0f) currentHealth = 0f;

        HealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
        {
            Destroyed?.Invoke();
            Debug.Log("Game Over: Military Base destroyed.");
            // Ethan Zhao add game over screen logic here
        }
    }
}
