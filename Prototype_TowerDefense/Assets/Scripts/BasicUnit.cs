using UnityEngine;

public class BasicUnit : MonoBehaviour
{
    [Header("UnitSats")]
    public int healthPoints = 40;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CalculateDamage(int damageAmount)
    {
        healthPoints -= damageAmount;
        if (healthPoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);

    }
}
