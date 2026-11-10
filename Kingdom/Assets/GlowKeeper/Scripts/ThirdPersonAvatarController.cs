
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonAvatarController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference runAction;
    public InputActionReference descendAction;

    [Header("Walking and Running")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float turnSpeed = 3f;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Swimming")]
    public float swimSpeed = 3f;
    public float swimVerticalSpeed = 2f;
    public float WaterSurface => FindWaterSurface();
    public bool IsSwimming => !float.IsNegativeInfinity(WaterSurface);

    private const float WaterEntryDepth = 0.08f;
    private const float SurfaceSwimDepth = 0.85f;
    private readonly HashSet<Collider> waterContacts = new HashSet<Collider>();
    private CharacterController controller;
    private float verticalVelocity;

    private bool isSwimming = false;
    private bool isJumping = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (runAction != null) runAction.action.Enable();
        if (descendAction != null) descendAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (runAction != null) runAction.action.Disable();
        if (descendAction != null) descendAction.action.Disable();
    }

    void Update()
    {
        if (cameraTransform == null || moveAction == null)
            return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * input.y + right * input.x;

        moveDirection =
            Vector3.ClampMagnitude(moveDirection, 1f);

        bool isMoving =
            moveDirection.sqrMagnitude > 0.001f;

        if (isMoving)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        bool runPressed =
            runAction != null &&
            runAction.action.IsPressed();

        bool goingUp =
            jumpAction != null &&
            jumpAction.action.IsPressed();

        bool goingDown =
            descendAction != null &&
            descendAction.action.IsPressed();

        float waterSurface = WaterSurface;
        bool inWater = !float.IsNegativeInfinity(waterSurface);
        if (isSwimming != inWater)
        {
            isSwimming = inWater;
            isJumping = false;
            verticalVelocity = 0f;
        }

        if (isSwimming)
        {
            HandleSwimming(moveDirection, goingUp, goingDown, waterSurface);
        }
        else
        {
            HandleLandMovement(moveDirection, runPressed);
        }

        // Update Animator parameters.
        if (animator != null)
        {
            bool isWalking =
                !isSwimming && isMoving;

            bool isRunning =
                isWalking && runPressed;

            // Swimming movement includes horizontal and vertical movement.
            bool isSwimMoving =
                isSwimming &&
                (isMoving || goingUp || goingDown);

            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsJumping", isJumping);

            animator.SetBool("IsSwimming", isSwimming);
            animator.SetBool("IsSwimMoving", isSwimMoving);
        }
    }

    void HandleLandMovement(
        Vector3 moveDirection,
        bool runPressed)
    {
        float currentSpeed =
            runPressed ? runSpeed : walkSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (controller.isGrounded &&
            jumpAction != null &&
            jumpAction.action.WasPressedThisFrame())
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);

            isJumping = true;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            moveDirection * currentSpeed +
            Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (isJumping &&
            controller.isGrounded &&
            verticalVelocity <= 0f)
        {
            isJumping = false;
        }
    }

    void HandleSwimming(
        Vector3 moveDirection,
        bool goingUp,
        bool goingDown,
        float waterSurface)
    {
        // Hold depth unless the player dives or swims up.
        float verticalMove = 0f;
        if (goingDown)
            verticalMove = -swimVerticalSpeed * Time.deltaTime;
        else if (goingUp)
            verticalMove = Mathf.Min(swimVerticalSpeed * Time.deltaTime,
                Mathf.Max(0f, waterSurface - SurfaceSwimDepth - transform.position.y));

        controller.Move(moveDirection * (swimSpeed * Time.deltaTime) + Vector3.up * verticalMove);
    }

    float FindWaterSurface()
    {
        // A capsule can touch the edge of a trigger while the player is still on shore.
        Vector3 sample = transform.position + Vector3.up * WaterEntryDepth;
        float surface = float.NegativeInfinity;
        foreach (Collider water in waterContacts)
        {
            if (water && water.enabled && water.gameObject.activeInHierarchy &&
                water.ClosestPoint(sample) == sample)
                surface = Mathf.Max(surface, water.bounds.max.y);
        }
        // The ocean trigger also covers the poison cove. Keep gravity there.
        if (!float.IsNegativeInfinity(surface) && Puzzle3PoisonWater.BlocksSwimming(sample))
            return float.NegativeInfinity;
        return surface;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
            waterContacts.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        waterContacts.Remove(other);
    }
}
