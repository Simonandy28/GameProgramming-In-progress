
using UnityEngine;
using UnityEngine.InputSystem;

public class Puzzle3Crystal : MonoBehaviour
{
    [SerializeField] private Puzzle3Ritual ritual;
    [SerializeField] private Transform player, hand, plate;
    [SerializeField] private Light offeringLight;
    [SerializeField] private InputActionReference interact;

    public bool Offered { get; private set; }
    public bool IsCarried => collected && !Offered;
    public bool CanInteract => isActiveAndEnabled && !Offered &&
        ritual && ritual.IsActive && player && hand && plate && offeringLight && interact &&
        Vector3.Distance(player.position, (collected ? plate : transform).position) <= 3f;
    private bool collected;
    private static int lastInteractionFrame = -1;

    private void Update()
    {
        if (!CanInteract ||
            lastInteractionFrame == Time.frameCount ||
            !interact.action.WasPressedThisFrame()) return;

        // Only one crystal can use this E press.
        lastInteractionFrame = Time.frameCount;
        if (!collected)
        {
            transform.SetParent(hand);
            transform.localPosition = Vector3.zero;
            collected = true;
        }
        else
        {
            transform.SetParent(plate);
            transform.localPosition = Vector3.up * 0.5f;
            offeringLight.enabled = true;
            Offered = true;
        }
    }
}
