using UnityEngine;

public class CamerMovement : MonoBehaviour
{
    public Transform camerPosition;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = camerPosition.position;   
    }
}
