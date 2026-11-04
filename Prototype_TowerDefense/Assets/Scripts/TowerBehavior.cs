using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TowerBehavior : MonoBehaviour
{
    [Header("Setup Fields")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Tower Stats")]
    // Shots per seconds
    public float fireRate = 1f;
    public int damage = 10;

    private SphereCollider sphereCollider;
    private List<GameObject> enemyUnitsInRange = new List<GameObject>();
    private GameObject currentTarget;
    private float fireCountdown = 0f;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        
    }

    void Update()
    {
        CleanUpUnitsInRange();
        currentTarget = GetNearestTarger();

        if (currentTarget != null)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
        }
        fireCountdown -= Time.deltaTime;
        
    }

    void CleanUpUnitsInRange()
    {
        // Remove Units that died
        enemyUnitsInRange.RemoveAll(unit => unit == null);
    }

    GameObject GetNearestTarger()
    {
        GameObject nearestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach(GameObject potentialTarget in enemyUnitsInRange)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                nearestTarget = potentialTarget;
            }
        }
    
        return nearestTarget;
    }

    void Shoot()
    {
        GameObject arrowGO = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        
        Projectile_Arrow arrow = arrowGO.GetComponent<Projectile_Arrow>();
        if (arrow != null)
        {   
            arrow.Initiate(currentTarget.transform, damage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(gameObject.tag))
        {
            enemyUnitsInRange.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (enemyUnitsInRange.Contains(other.gameObject))
        {
            enemyUnitsInRange.Remove(other.gameObject);
        }
    }
}
