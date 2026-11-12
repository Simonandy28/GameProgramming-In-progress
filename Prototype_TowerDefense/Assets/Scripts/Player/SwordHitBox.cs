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
        if (other.isTrigger) return;

        IDamageable unit = other.GetComponentInParent<IDamageable>();
        Component owner = unit as Component;
        if (owner == null || owner.CompareTag("Ally") || owner.CompareTag("Player")) return;   // don't hit your own team

        if (!unitsHitThisAttack.Contains(other))
        {
            unitsHitThisAttack.Add(other);
            unit.TakeDamage(20);
        }
    }

}
