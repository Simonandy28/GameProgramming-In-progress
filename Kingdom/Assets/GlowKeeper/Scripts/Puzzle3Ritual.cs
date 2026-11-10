
using UnityEngine;

public class Puzzle3Ritual : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private Light[] stoneLights;
    [SerializeField] private Light plateLight;
    public bool IsActive => activated && IsNight;
    public bool PlayerOnPlate => playerOnPlate;
    private bool playerOnPlate;
    private bool activated;

    private bool IsNight => sun && sun.transform.forward.y > 0;

    private void Start()
    {
        SetLights(false);
    }

    private void Update()
{
    if (!IsNight)
        activated = false;
    else if (playerOnPlate)
        activated = true;

    SetLights(activated);

    if (plateLight)
        plateLight.enabled = playerOnPlate && IsNight;
}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerOnPlate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerOnPlate = false;
    }

    private void SetLights(bool value)
    {
        foreach (Light light in stoneLights)
            if (light) light.enabled = value;
    }
}
