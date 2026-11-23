using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class MouseMovement : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mousesensitivity = 1f;

    [Header("Player Body")]
    public Transform playerBody;

    private InputSystem_Actions inputSystem;    
    private float xRotation = 0f;
    private Vector2 mouseLook;
    

    private void Awake()
    {    
        inputSystem = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerBody == null) return;
        Look();
    }

    private void Look()
    {
        mouseLook = inputSystem.Player.Look.ReadValue<Vector2>();

        float mouseX = mouseLook.x * mousesensitivity;
        float mouseY = mouseLook.y * mousesensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(Vector3.up * mouseX);
        transform.rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
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
