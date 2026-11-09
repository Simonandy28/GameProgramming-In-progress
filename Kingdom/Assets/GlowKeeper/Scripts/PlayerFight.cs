using UnityEngine;

public class PlayerFight : MonoBehaviour
{
    [SerializeField] float maxHP = 10;
    [HideInInspector] public float currentHP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void attackedByEnemy(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Debug.Log("GameOver");
        }
    }
}
