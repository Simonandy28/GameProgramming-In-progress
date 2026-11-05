using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class EnemyController : MonoBehaviour
{
    public GameObject player;
    [SerializeField] float speed = 1f;
    private CharacterController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = this.gameObject.GetComponent<CharacterController>();
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
}
