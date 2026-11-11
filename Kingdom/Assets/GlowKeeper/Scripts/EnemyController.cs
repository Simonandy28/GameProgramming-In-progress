using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    public GameObject player;
    private PlayerFight playerFight;
    public EnemySpawner spawner;
    [SerializeField] Animator animator;
    public float maxHP = 3;
    public float currentHP;
    [SerializeField] float damage = 1;
    private float attackTimer = 0;
    [SerializeField] float attackCooldown = 2;
    private bool canAttack = true;

    Vector3 destination;
    NavMeshAgent agent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
        animator.SetBool("IsWalking", true);
        playerFight = player.GetComponent<PlayerFight>();

        agent = GetComponent<NavMeshAgent>();
        destination = agent.destination;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
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

        // Update destination if the target moves one unit
        if (Vector3.Distance(destination, player.transform.position) > 1.0f)
        {
            destination = player.transform.position;
            agent.destination = destination;
        }
        if (agent.remainingDistance <= 2.5f)
        {
            playerAtRange();
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

    void playerAtRange()
    {
        if (!canAttack)
        {
            return;
        }

        if ( player != null)
        {
            animator.SetTrigger("IsAttacking");
            playerFight.attackedByEnemy(damage);
            canAttack = false;
        }
    }
}
