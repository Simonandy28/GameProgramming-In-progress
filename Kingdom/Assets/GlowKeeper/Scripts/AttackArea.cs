using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public List<EnemyController> atRange = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("smth entered");
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            Debug.Log("its an enemy");
            atRange.Add(enemy);
        }
    }

    void OnTriggerExit(Collider other)
    {
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            atRange.Remove(enemy);
        }
    }
}
