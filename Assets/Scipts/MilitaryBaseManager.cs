using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MilitaryBase : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Scene Flow")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    private float currentHealth;
    private bool gameOverTriggered;

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

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f && !gameOverTriggered)
        {
            gameOverTriggered = true;
            Destroyed?.Invoke();

            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
