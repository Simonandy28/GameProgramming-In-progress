using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class EnemyController : MonoBehaviour
{
    public GameObject player;
    public EnemySpawner spawner;
    [SerializeField] float speed = 1f;
    private CharacterController controller;
    public float maxHP = 3;
    private float currentHP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = this.gameObject.GetComponent<CharacterController>();
        currentHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(player != null)
        {
            Vector3 moveDirection = (player.transform.position - this.transform.position).normalized;
            moveDirection.y = 0f;
            Vector3 velocity =
            moveDirection * speed;

            controller.Move(velocity * Time.deltaTime);
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
}
