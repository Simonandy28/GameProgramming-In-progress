using UnityEngine;
using UnityEngine.InputSystem;

public class UnderwaterCrystalAttraction : MonoBehaviour
{
    public Transform player;
    public Transform handTarget;
    public InputActionReference interactAction;
    public float pickupDistance = 3f;
    public float moveSpeed = 5f;

    private bool attracted;
    private bool collected;

    public bool CanCollect => isActiveAndEnabled && !attracted && !collected &&
        player && handTarget && interactAction && interactAction.action.enabled &&
        Vector3.Distance(player.position, transform.position) <= pickupDistance;

    void Update()
    {
        if (!player || !handTarget || !interactAction) return;

        if (CanCollect && interactAction.action.WasPressedThisFrame())
        {
            attracted = true;
        }

        if (attracted && !collected)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                handTarget.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, handTarget.position) < 0.1f)
            {
                collected = true;
                attracted = false;
                transform.SetParent(handTarget);
                transform.localPosition = Vector3.zero;
            }
        }
    }
}
