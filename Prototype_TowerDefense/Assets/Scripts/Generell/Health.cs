using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth { get; private set; }
    public bool IsAlive => currentHealth > 0; 

    [Tooltip("If true, this GameObject is destroyed automatically when health reaches 0.")]
    public bool destroyOnDeath = true;

    public event Action<int, int> OnHealthChanged; 
    public event Action OnDeath;
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= damageAmount;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke(); 

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}