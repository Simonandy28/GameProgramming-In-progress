using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IdyllicFantasyNature
{
    public class CameraMovement : MonoBehaviour
    {
        [Range(1f, 10f)]
        [Tooltip("speed of the camera movement")]
        [SerializeField] private float _mouseSensity = 1;

        // mouse rotation
        private float _xRotation;
        private float _yRotation;

        [Tooltip("the parent of this object")]
        [SerializeField] private Transform _controller;

        // Start is called before the first frame update
        private void Start()
        {
            // locks cursor and makes it invisible
            _yRotation = transform.eulerAngles.y;
            _xRotation = Mathf.DeltaAngle(0, transform.eulerAngles.x);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Update is called once per frame
        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame && Application.isFocused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            Vector2 delta = mouse.delta.ReadValue() * 0.05f;
            float mouseX = delta.x * _mouseSensity;
            float mouseY = delta.y * _mouseSensity;
#else
            if (Input.GetMouseButtonDown(0)) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensity;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensity;
#endif

            _yRotation += mouseX;
            _xRotation -= mouseY;

            // limits camera rotation
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            // rotates camera on the y- and x-axis
            transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

            // rotates the controller on the y-axis so that it is on the same rotation as the camera
            _controller.rotation = Quaternion.Euler(0, _yRotation, 0);
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
