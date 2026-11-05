using UnityEngine;
using UnityEngine.InputSystem;

public class FireflyHandAttraction : MonoBehaviour
{
    [Header("Scene references")]
    public Transform player;
    public Transform beeHandTarget;
    public Animator playerAnimator;
    public InputActionReference interactAction;
    [Tooltip("The three Firefly_Fred models inside the animated holders.")]
    public Transform[] fireflies = new Transform[3];

    [Header("Settings")]
    [Min(0.1f)] public float interactionDistance = 3f;
    [Min(0.1f)] public float movementSpeed = 5f;
    [Range(0.1f, 1f)] public float handFormationSize = 0.3f;
    [Min(0.1f)] public float stoneFormationSize = 1.5f;

    public bool IsAtHand { get; private set; }
    public bool IsAtStone { get; private set; }
    public bool HasLeftPlant => attracted;
    public bool CanAttract => !attracted && player &&
        Vector3.Distance(player.position, transform.position) <= interactionDistance;

    private bool attracted;
    private Vector3 remainingOffset;
    private float formationSize = 1f;
    private Transform currentTarget;
    private bool goingToStone;

    private void OnEnable()
    {
        if (interactAction) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction) interactAction.action.Disable();
    }

    private void Update()
    {
        if (!player || !beeHandTarget || !interactAction) return;

        if (CanAttract && interactAction.action.WasPressedThisFrame())
        {
            attracted = true; // E works only once for this swarm.
            currentTarget = beeHandTarget;
            remainingOffset = transform.position - beeHandTarget.position;
            if (playerAnimator) playerAnimator.SetTrigger("AttractFireflies");
        }
    }

    public void FlyToStone(Transform stoneTarget)
    {
        if (!IsAtHand || !stoneTarget) return;
        // Play the two-handed spell as the bees leave the hand.
        if (playerAnimator) playerAnimator.SetTrigger("ReleaseFireflies");
        IsAtHand = false;
        goingToStone = true;
        currentTarget = stoneTarget;
        // Remember the current gap so changing destinations never teleports the bees.
        remainingOffset = transform.position - currentTarget.position;
    }

    private void LateUpdate()
    {
        if (!attracted || !currentTarget) return;

        // Close the gap while following the animated hand. Never change parents or scales.
        remainingOffset = Vector3.MoveTowards(remainingOffset, Vector3.zero, movementSpeed * Time.deltaTime);
        transform.position = currentTarget.position + remainingOffset;
        float desiredSize = goingToStone ? stoneFormationSize : handFormationSize;
        formationSize = Mathf.MoveTowards(formationSize, desiredSize, Time.deltaTime);

        if (remainingOffset == Vector3.zero)
        {
            IsAtHand = !goingToStone;
            IsAtStone = goingToStone;
        }

        // Each Animator has just updated its local flight position. Make that same
        // flight smaller around the hand, preserving its phase, speed and model size.
        foreach (Transform firefly in fireflies)
            if (firefly) firefly.localPosition *= formationSize;
    }
}
