using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Area")]
    public GameObject attackArea;

    [Header("Attack Settings")]
    public float attackDuration = 0.5f; 

    private bool isAttacking = false;
    private float timer = 0f;

    private InputSystem_Actions inputSystem;

    void Awake()
    {
        inputSystem = new InputSystem_Actions();
    }

    void Start()
    {
        attackArea.SetActive(isAttacking);
    }

    void Update()
    {
        bool attack = inputSystem.Player.Attack.triggered;

        if (attack && !isAttacking)
        { 
            isAttacking = true;
            Attack();   
        } 

        if (isAttacking)
        {
            timer += Time.deltaTime;
            if (timer >= attackDuration)
            {
                isAttacking = false;
                timer = 0f;
                attackArea.SetActive(isAttacking);    
            }
        }
    }

    private void Attack()
    {
        Debug.Log("Attack initiated");
        attackArea.SetActive(isAttacking);
    }

    void OnEnable()
    {
        inputSystem.Enable();
    }

    void OnDisable()
    {
        inputSystem.Disable();
    }
}


