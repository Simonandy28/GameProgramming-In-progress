
using UnityEngine;

public class LampReceiver : MonoBehaviour
{
    [Header("Firefly Destination")]
    public Transform fireflyDestination;

    [Header("Lamp Light")]
    public Light magicLampLight;

    private bool isLit = false;

    public bool IsLit
    {
        get { return isLit; }
    }

    void Start()
    {
        // The lamp starts dark.
        if (magicLampLight != null)
            magicLampLight.enabled = false;
    }

    public void LightUp()
    {
        if (isLit)
            return;

        isLit = true;

        if (magicLampLight != null)
            magicLampLight.enabled = true;

        Debug.Log(gameObject.name + " is now glowing!");
    }
}
