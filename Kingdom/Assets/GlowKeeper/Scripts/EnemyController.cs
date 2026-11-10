using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class EnemyController : MonoBehaviour
{
    public GameObject player;
    public EnemySpawner spawner;
    [SerializeField] float speed = 1f;
    [SerializeField] float turnSpeed = 3f;
    [SerializeField] Animator animator;
    private CharacterController controller;
    public float maxHP = 3;
    private float currentHP;
    [SerializeField] float damage = 1;
    private float attackTimer = 0;
    [SerializeField] float attackCooldown = 2;
    private bool canAttack = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = this.gameObject.GetComponent<CharacterController>();
        currentHP = maxHP;
        animator.SetBool("IsWalking", true);
    }

    // Update is called once per frame
    void Update()
    {
        
        if(player != null)
        {
            Vector3 moveDirection = (player.transform.position - this.transform.position).normalized;
            moveDirection.y = 0f;
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
            Vector3 velocity =
            moveDirection * speed;

            controller.Move(velocity * Time.deltaTime);
        }
        if(attackTimer < attackCooldown && canAttack == false)
        {
            attackTimer += Time.deltaTime;
        }
        if (attackTimer >= attackCooldown && canAttack == false)
        {
            attackTimer = 0;
            canAttack = true;
        }
    }

    public void Damaged(float damage)
    {
        currentHP -= damage;
        if(currentHP <= 0)
        {
            spawner.destroyEnemy(this);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!canAttack)
        {
            return;
        }
        CharacterController otherController = hit.collider.GetComponent<CharacterController>();

        if (otherController == null)
        {
            return;
        }
        
        PlayerFight player = otherController.gameObject.GetComponent<PlayerFight>();
        if ( player != null)
        {
            animator.SetTrigger("IsAttacking");
            player.attackedByEnemy(damage);
            canAttack = false;
        }
    }
}
