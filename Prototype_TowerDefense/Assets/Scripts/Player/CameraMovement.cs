using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform camerPosition;

    // Update is called once per frame
    void LateUpdate()
    {
        if (camerPosition != null) 
        {
            transform.position = camerPosition.position;   
        }
    }
}
