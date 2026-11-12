using UnityEngine;

public class UnitBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.2f;

    [Header("Crowd Avoidance (Flanking)")]
    [Tooltip("Radius to push away from nearby friendly units so they surround targets instead of queuing behind each other.")]
    public float separationRadius = 0.9f;
    public float separationStrength = 2.0f;

    [Header("Combat & Targeting")]

    [Tooltip("Type 'Enemy' for Allies, and 'Ally' for Enemies")]
    public string targetTag = "Enemy";
    [Tooltip("If true, Enemies looking for 'Ally' will also target objects tagged 'Player'")]
    public bool includePlayerTag = true;
    public float detectionRange = 15f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;

    private Animator animator;
    private Transform currentTarget;
    private Collider currentTargetCollider;
    private IDamageable currentTargetDamageable;
    private bool isCurrentTargetTower;

    private Vector3? moveDestination;
    private float attackTimer;
    private float scanTimer;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Scan for targets and handle movement/attacking
    // Attacking has priority over moving to a destination
    private void Update()
    {
        attackTimer -= Time.deltaTime;
        scanTimer -= Time.deltaTime;

        if (!IsTargetValid())
        {
            ClearTarget();
            ScanForTargets();
            scanTimer = 0.4f;
        } else if (scanTimer <= 0f)
        {
            scanTimer = 0.2f;
            ScanForTargets();
        }

        if (IsTargetValid())
        {
            EngageTarget();
        }
        else
        {
            MoveToDestination();
        }
    }

    private bool IsTargetValid()
    {
        if (currentTarget == null || currentTargetCollider == null || currentTargetDamageable == null)
            return false;
        
        if (!currentTargetCollider.enabled || !currentTarget.gameObject.activeInHierarchy)
            return false;

        if (!currentTargetDamageable.IsAlive)
            return false;

        if (!IsValidTargetTag(currentTarget.gameObject) && !IsValidTargetTag(currentTarget.root.gameObject))
            return false;

        if (isCurrentTargetTower)
        {
            TowerBehavior tower = currentTarget.GetComponentInParent<TowerBehavior>();
            if (tower != null && tower.IsDestroyed)
                return false;
        }

        Vector3 closestPoint = GetClosestPointSafe(currentTargetCollider, transform.position);
        if(GetFlatDistance(transform.position, closestPoint) > detectionRange * 1.25f)
            return false;
    
        return true;
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetCollider = null;
        currentTargetDamageable = null;
        isCurrentTargetTower = false;
    }

    // Consider using target Layer for enemy detection
    // This method finds the closest valid target (unit or tower) within detection range
    private void ScanForTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, ~0, QueryTriggerInteraction.Ignore);

        Collider closestUnit = null;
        float closestUnitScore = Mathf.Infinity;

        Collider closestTower = null;
        float closestTowerDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) continue;

            Component damageableComp = damageable as Component;
            
            TowerBehavior tower = hit.GetComponentInParent<TowerBehavior>();
            bool isTower = tower != null;

            if (isTower)
            {
                // Ignore destroyed towers and any collider other than the tower's activeCollider
                if (tower.IsDestroyed || hit != tower.activeCollider) continue;

                if (!IsValidTargetTag(tower.gameObject)) continue;
            }
            else
            {
                // For units/player, only target the main collider on the IDamageable object (ignore child weapon colliders)
                if (damageableComp == null || hit.gameObject != damageableComp.gameObject) continue;

                if (!IsValidTargetTag(damageableComp.gameObject) && !IsValidTargetTag(hit.transform.root.gameObject))
                {
                    continue;
                }
            }

            Vector3 closestPoint = GetClosestPointSafe(hit, transform.position);
            float dist = GetFlatDistance(transform.position, closestPoint);

            if (!isTower)
            {
                float score = dist;
                if (damageable == currentTargetDamageable)
                {
                    score -= 0.5f;
                }

                if (score < closestUnitScore)
                {
                    closestUnitScore = score;
                    closestUnit = hit;
                }
            }
            else if (dist < closestTowerDist)
            {
                closestTowerDist = dist;
                closestTower = hit;
            }
        }

        // Priority 1: Other Units. Priority 2: Towers.
        Collider chosenCollider = closestUnit != null ? closestUnit : closestTower;

        if (chosenCollider != null)
        {
            currentTargetCollider = chosenCollider;
            currentTargetDamageable = chosenCollider.GetComponentInParent<IDamageable>();
            currentTarget = (currentTargetDamageable as Component != null) 
                ? (currentTargetDamageable as Component).transform 
                : chosenCollider.transform;
            isCurrentTargetTower = chosenCollider.GetComponentInParent<TowerBehavior>() != null;
        }
        else
        {
            ClearTarget();
        }
    }

    private bool IsValidTargetTag(GameObject obj)
    {
        if (obj.CompareTag(targetTag)) return true;

        if (includePlayerTag && targetTag == "Ally" && obj.CompareTag("Player"))
            return true;

        return false;
    }

    // Attack or move towards the current target
    private void EngageTarget()
    {
        // Distance from unit to the outer surface of the target collider
        Vector3 targetSurfacePoint = GetClosestPointSafe(currentTargetCollider, transform.position);
        float distanceToSurface = GetFlatDistance(transform.position, targetSurfacePoint);

        if (distanceToSurface <= attackRange)
        {
            SetAnimatorSpeed(0f);
            LookAtFlat(currentTarget.position);

            if (attackTimer <= 0f)
            {
                attackTimer = attackCooldown;
                if (animator != null) animator.SetTrigger("Attack");
                currentTargetDamageable.TakeDamage(attackDamage);
            }
        }
        else
        {
            StepTowards(targetSurfacePoint, true);
        }
    }

    private void MoveToDestination()
    {
        if (moveDestination == null)
        {
            SetAnimatorSpeed(0f);
            return;
        }

        Vector3 flatTarget = new Vector3(moveDestination.Value.x, transform.position.y, moveDestination.Value.z);
        float distance = Vector3.Distance(transform.position, flatTarget);

        if (distance <= stoppingDistance)
        {
            moveDestination = null;
            SetAnimatorSpeed(0f);
        }
        else
        {
            StepTowards(moveDestination.Value, true);
        }
    }

    // Stepping towards a destination
    private void StepTowards(Vector3 targetPoint, bool useAvoidance)
    {
       Vector3 desiredDir = targetPoint - transform.position;
        desiredDir.y = 0f;

        if (desiredDir.sqrMagnitude < 0.0001f) return;
        desiredDir.Normalize();

        // Crowd Separation: Gently steer around nearby teammates so multiple units 
        // wrap around a target instead of getting stuck behind the first attacker
        if (useAvoidance && separationRadius > 0f)
        {
            Vector3 separation = Vector3.zero;
            Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, ~0, QueryTriggerInteraction.Ignore);

            foreach (Collider neighbor in neighbors)
            {
                if (neighbor.transform.IsChildOf(transform)) continue;
                if (neighbor == currentTargetCollider) continue; 

                if (neighbor.GetComponentInParent<UnitBehavior>() == null) continue;

                Vector3 pushDir = transform.position - neighbor.transform.position;
                pushDir.y = 0f;
                float dist = pushDir.magnitude;

                if (dist > 0.001f && dist < separationRadius)
                {
                    separation += (pushDir.normalized / dist) * (separationRadius - dist);
                }
            }

            desiredDir = (desiredDir + separation * separationStrength).normalized;
        }

        Vector3 nextPos = transform.position + desiredDir * (moveSpeed * Time.deltaTime);
        LookAtFlat(transform.position + desiredDir);
        transform.position = nextPos;
        SetAnimatorSpeed(moveSpeed);
    }

    // Safely gets the closest point even if a collider is a non-convex MeshCollider
    private Vector3 GetClosestPointSafe(Collider col, Vector3 fromPos)
    {
        if (col is BoxCollider || col is SphereCollider || col is CapsuleCollider || (col is MeshCollider mc && mc.convex))
        {
            return col.ClosestPoint(fromPos);
        }
        return col.bounds.ClosestPoint(fromPos);
    }

    // Calculates horizontal distance on the XZ plane, ignoring height differences
    private float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void LookAtFlat(Vector3 targetPoint)
    {
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0f; // Keep unit upright

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void SetAnimatorSpeed(float currentSpeed)
    {
        if (animator != null)
        {
            float normalizedSpeed = currentSpeed / moveSpeed;
            normalizedSpeed = Mathf.Clamp(normalizedSpeed, -1f, 1f);

            animator.SetFloat("Speed", normalizedSpeed);
        }
    }

    // Called by the Allied or Enemy Brain scripts
    public void SetMoveDestination(Vector3 destination)
    {
        moveDestination = destination;
    }

    public bool HasReachedDestination()
    {
        return moveDestination == null;
    }

    // Visualizes ranges in the Scene View when the unit is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}