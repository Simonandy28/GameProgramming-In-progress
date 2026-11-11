using UnityEngine;

public class Projectile_Arrow : MonoBehaviour
{
    public float speed = 15f;
    private Transform target;
    private int damage;
    

    public void Initiate(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
    }    
    
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
    
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);        
    }

    void HitTarget()
    {
        IDamageable targetHealth = target.GetComponentInParent<IDamageable>();

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
