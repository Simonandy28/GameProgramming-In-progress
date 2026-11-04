
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Patrol")]
    public float patrolRadius = 10f;
    public float patrolSpeed = 1.5f;
    public float waitTime = 2f;

    [Header("Vision")]
    public float visionDistance = 12f;

    [Range(0f, 360f)]
    public float visionAngle = 100f;

    public float chaseSpeed = 5f;

    [Header("Attack")]
    public float attackDistance = 2f;
    public float hitDistance = 2.6f;
    public float attackHitDelay = 0.65f;
    public float attackCooldown = 1.2f;

    [Header("Animation")]
    public Animator animator;

    private NavMeshAgent agent;
    private PlayerDeath playerHealth;

    private Vector3 startPosition;
    private float waitTimer;

    private bool isWaiting = true;
    private bool isChasing = false;
    private bool isAttacking = false;

    private string currentAnimation = "";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player != null)
            playerHealth = player.GetComponent<PlayerDeath>();

        if (playerHealth == null)
            Debug.LogWarning("PlayerDeath missing on PlayerRoot!");

        startPosition = transform.position;

        agent.speed = patrolSpeed;
        agent.stoppingDistance = 1.5f;

        waitTimer = waitTime;

        PlayAnimation("Idel");
    }

    void Update()
    {
        if (!agent.isOnNavMesh || isAttacking)
            return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            agent.isStopped = true;
            PlayAnimation("Idel");
            return;
        }

        if (player != null)
        {
            float distance = HorizontalDistanceToPlayer();

            if (isChasing && distance <= attackDistance)
            {
                StartCoroutine(AttackPlayer());
                return;
            }

            if (CanSeePlayer())
            {
                if (distance <= attackDistance)
                    StartCoroutine(AttackPlayer());
                else
                    ChasePlayer();

                return;
            }
        }

        if (isChasing)
        {
            isChasing = false;

            agent.ResetPath();
            agent.isStopped = true;

            isWaiting = true;
            waitTimer = waitTime;

            PlayAnimation("Idel");
        }

        HandlePatrol();
    }

    float HorizontalDistanceToPlayer()
    {
        Vector3 difference =
            player.position - transform.position;

        difference.y = 0f;

        return difference.magnitude;
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.magnitude > visionDistance)
            return false;

        float angle = Vector3.Angle(
            transform.forward,
            direction
        );

        if (angle > visionAngle / 2f)
            return false;

        Vector3 eyePosition =
            transform.position + Vector3.up * 1.5f;

        Vector3 targetPosition =
            player.position + Vector3.up;

        Vector3 sightDirection =
            targetPosition - eyePosition;

        if (Physics.Raycast(
            eyePosition,
            sightDirection.normalized,
            out RaycastHit hit,
            sightDirection.magnitude,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player ||
                   hit.transform.IsChildOf(player);
        }

        return true;
    }

    void ChasePlayer()
    {
        isChasing = true;

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        agent.SetDestination(player.position);

        PlayAnimation("Running");
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        isChasing = true;

        agent.isStopped = true;
        agent.ResetPath();

        // Turn to face the wizard.
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        PlayAnimation("Attacking");

        // Wait for the attack to reach its hit moment.
        yield return new WaitForSeconds(attackHitDelay);

        if (player != null &&
            playerHealth != null &&
            !playerHealth.IsDead)
        {
            float distance = HorizontalDistanceToPlayer();

            if (distance <= hitDistance)
            {
                Debug.Log("Zombie landed a hit!");

                // Remove one health point.
                playerHealth.TakeDamage(1);
            }
            else
            {
                Debug.Log("Wizard escaped the attack!");
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        currentAnimation = "";

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    void HandlePatrol()
    {
        agent.speed = patrolSpeed;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
                FindPatrolPoint();

            return;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = waitTime;

            agent.isStopped = true;

            PlayAnimation("Idel");
        }
    }

    void FindPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint =
                startPosition +
                Random.insideUnitSphere * patrolRadius;

            if (NavMesh.SamplePosition(
                randomPoint,
                out NavMeshHit hit,
                2f,
                NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(hit.position, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.isStopped = false;
                    agent.speed = patrolSpeed;

                    agent.SetDestination(hit.position);

                    isWaiting = false;

                    PlayAnimation("Walking");
                    return;
                }
            }
        }

        waitTimer = waitTime;
    }

    void PlayAnimation(string animationName)
    {
        if (animator == null)
            return;

        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;

        animator.CrossFade(animationName, 0.15f);
    }
}
