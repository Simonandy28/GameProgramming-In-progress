using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TowerBehavior : MonoBehaviour
{
    [Header("Setup Fields")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public GameObject inactiveVisual;
    public GameObject activeVisual;
    public Collider inactiveCollider;
    public Collider activeCollider;


    [Header("Tower Stats")]
    // Shots per seconds
    public Health health;
    public float fireRate = 1f;
    public int damage = 10;

    private SphereCollider shootingRangeCollider;
    private List<GameObject> enemyUnitsInRange = new List<GameObject>();
    private GameObject currentTarget;
    private float fireCountdown = 0f;
    private bool isDestroyed = false;

    public bool IsDestroyed => isDestroyed;

    void Start()
    {
        activeVisual.SetActive(true);
        inactiveVisual.SetActive(false);

        activeCollider.enabled = true;
        inactiveCollider.enabled = false;

        shootingRangeCollider = GetComponent<SphereCollider>();
        shootingRangeCollider.isTrigger = true;

        health.OnDeath += DestroyTower;
        
    }

    void Update()
    {
        if (isDestroyed) return;

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
        bool otherIsAlly = other.CompareTag("Ally") || other.CompareTag("Player");

        if ((gameObject.tag == "Ally" && other.CompareTag("Enemy")) || (gameObject.tag == "Enemy" && otherIsAlly))
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

    void DestroyTower()
    {
        isDestroyed = true;
        inactiveVisual.SetActive(true);
        activeVisual.SetActive(false);

        activeCollider.enabled = false;
        if (inactiveCollider != null) 
        {
            inactiveCollider.enabled = true;
        }

        if (shootingRangeCollider != null)
        {
            shootingRangeCollider.enabled = false;
        }

        gameObject.tag = "DestroyedTower";
    }
}
