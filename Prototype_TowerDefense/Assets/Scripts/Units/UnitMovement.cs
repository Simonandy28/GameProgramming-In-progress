using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class UnitMovement : MonoBehaviour
{
    [Header("Unit Settings")]
    public float moveSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Ground Settings")]
    public Transform ground;
    public LayerMask groundMask;
    public float distanceToGround = 0.4f;

    [Header("Animation")]
    public Animator animator;


    private InputSystem_Actions inputSystem;
    private Vector3 velocity;
    private Vector2 move;
    private CharacterController controller;
    private bool isGrounded;

    private void Awake()
    {        
        inputSystem = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(ground.position, distanceToGround, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        move = inputSystem.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * move.x + transform.forward * move.y;

        velocity.y += gravity * Time.deltaTime;
        moveDirection *= moveSpeed;
        moveDirection.y = velocity.y;

        controller.Move(moveDirection * Time.deltaTime);
    
        if (animator == null) return;

        float forwardSpeed = Mathf.Abs(move.y) > 0.01f ? move.y : move.magnitude;
        animator.SetFloat("Speed", forwardSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
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