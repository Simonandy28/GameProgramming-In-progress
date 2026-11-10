using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Area")]
    public GameObject attackArea;

    [Header("Animation")]
    public Animator animator;

    private bool isAttacking = false;
    private float timer = 0f;

    private InputSystem_Actions inputSystem;

    void Awake()
    {
        inputSystem = new InputSystem_Actions();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        attackArea.SetActive(isAttacking);
    }

    void Update()
    {
        if (inputSystem.Player.Attack.WasPressedThisFrame() && !isAttacking)
        { 
            isAttacking = true;
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        } 

      
    }

    public void EnableHitBox()
    {
        attackArea.SetActive(true);
    }

    public void DisableHitBox()
    {
        attackArea.SetActive(false);
    }

    public void EndAttack()
    {
        isAttacking = false;
        attackArea.SetActive(false);
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


