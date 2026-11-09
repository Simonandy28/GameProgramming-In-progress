using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 10f;
    public float gravity = -9.81f;
    private float JumpHeight = 2.4f;

    [Header("Ground Settings")]
    public Transform ground;
    public LayerMask groundMask;
    public float distanceToGround = 0.4f;


    private InputSystem_Actions inputSystem;
    private Vector3 velocity;
    private Vector2 move;
    private CharacterController controller;
    private bool isGrounded;

    private void Awake()
    {        
        inputSystem = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Gravity();
        Movement();
        Jump();
    }

    private void Gravity()
    {
        isGrounded = Physics.CheckSphere(ground.position, distanceToGround, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Movement()
    {
        move = inputSystem.Player.Move.ReadValue<Vector2>();

        Vector3 distance = transform.right * move.x + transform.forward * move.y;
        controller.Move(distance * moveSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        if (inputSystem.Player.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity);
        }
    }

    private void OnEnable()
    {
        inputSystem.Enable();
    }

    private void OnDisable()
    {
        inputSystem.Disable();
    }
}