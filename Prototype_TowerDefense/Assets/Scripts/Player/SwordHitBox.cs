using UnityEngine;
using System.Collections.Generic;

public class SwordHitBox : MonoBehaviour
{
    private HashSet<BasicUnit> unitsHitThisAttack = new HashSet<BasicUnit>();

    private void OnEnable()
    {
        unitsHitThisAttack.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        BasicUnit unit = other.GetComponentInParent<BasicUnit>();
        
        if (unit != null && !unitsHitThisAttack.Contains(unit))
        {
            unitsHitThisAttack.Add(unit); 
            unit.CalculateDamage(10); 
            
            Debug.Log($"Damaged: {unit.gameObject.name}");
        }
    }
}
