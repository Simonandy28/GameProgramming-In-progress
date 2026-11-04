

using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 4f;
    public float height = 1.5f;
    public float mouseSensitivity = 0.15f;

    [Header("Vertical Rotation Limits")]
    public float minPitch = -30f;
    public float maxPitch = 70f;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start()
    {
        LockCursor();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Press Escape to release the mouse cursor.
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }

        // Click the Game view to lock the cursor again.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        // Read mouse movement using Unity's Input System.
        if (Mouse.current != null &&
            Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta =
                Mouse.current.delta.ReadValue();

            // Horizontal camera rotation.
            yaw += mouseDelta.x * mouseSensitivity;

            // Vertical camera rotation.
            pitch -= mouseDelta.y * mouseSensitivity;

            // Prevent the camera from rotating too far vertically.
            pitch = Mathf.Clamp(
                pitch,
                minPitch,
                maxPitch
            );
        }

        // Create the camera's rotation.
        Quaternion rotation =
            Quaternion.Euler(pitch, yaw, 0f);

        // Point around which the camera rotates.
        Vector3 pivotPosition =
            target.position + Vector3.up * height;

        // Place the camera behind the pivot.
        Vector3 cameraOffset =
            rotation * new Vector3(0f, 0f, -distance);

        transform.position =
            pivotPosition + cameraOffset;

        // Always look toward the wizard.
        transform.LookAt(pivotPosition);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        UnlockCursor();
    }
}
