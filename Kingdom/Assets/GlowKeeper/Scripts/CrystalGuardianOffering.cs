using UnityEngine;
using UnityEngine.InputSystem;

public class CrystalGuardianOffering : MonoBehaviour
{
    public Transform player;
    public Transform crystal;
    public Transform offeringTarget;
    public InputActionReference interactAction;
    public Light eyeGlow;
    public AudioSource hum;

    public float interactDistance = 3f;
    public float moveSpeed = 5f;

    private bool offering;
    private bool completed;

    void Update()
    {
        if (completed || !player || !crystal || !offeringTarget || !interactAction)
            return;

        if (!offering && crystal.parent != null && crystal.parent.name == "CrystalHandTarget" &&
            Vector3.Distance(player.position, transform.position) <= interactDistance &&
            interactAction.action.WasPressedThisFrame())
        {
            offering = true;
            crystal.SetParent(null);
        }

        if (offering)
        {
            crystal.position = Vector3.MoveTowards(
                crystal.position,
                offeringTarget.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(crystal.position, offeringTarget.position) < 0.1f)
            {
                crystal.SetParent(offeringTarget);
                crystal.localPosition = Vector3.zero;
                completed = true;
                if (eyeGlow) eyeGlow.enabled = true;
                if (hum) hum.Play();
            }
        }
    }
}