
using UnityEngine;

public class FireflyDirection : MonoBehaviour
{
    public Transform flightMotion;

    private Vector3 previousPosition;

    void Start()
    {
        if (flightMotion != null)
        {
            previousPosition = flightMotion.position;
        }
    }

    void LateUpdate()
    {
        if (flightMotion == null)
        {
            return;
        }

        // Find the direction the firefly is moving.
        Vector3 direction = flightMotion.position - previousPosition;

        // Keep the firefly upright.
        direction.y = 0f;

        // Rotate only when the firefly is moving.
        if (direction.sqrMagnitude > 0.000001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                8f * Time.deltaTime
            );
        }

        // Remember the position for the next frame.
        previousPosition = flightMotion.position;
    }
}
