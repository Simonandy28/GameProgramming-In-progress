using UnityEngine;
using System.Collections.Generic;

public class SwordHitBox : MonoBehaviour
{
    private HashSet<Collider> unitsHitThisAttack = new HashSet<Collider>();

    private void OnEnable()
    {
        unitsHitThisAttack.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ally") || other.CompareTag("Player") || other.isTrigger) return;

        IDamageable unit = other.GetComponentInParent<IDamageable>();
        
        if (unit != null && !unitsHitThisAttack.Contains(other))
        {
            unitsHitThisAttack.Add(other); 
            unit.TakeDamage(20); 
        }
    }
}
