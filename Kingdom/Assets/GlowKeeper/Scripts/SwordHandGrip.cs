using UnityEngine;

// Close only the right-hand fingers while this sword is held.
public class SwordHandGrip : MonoBehaviour
{
    public FloatingSword sword;
    [SerializeField] private Transform[] fingers;
    [SerializeField] private Quaternion[] closedRotations;

    private PlayerSwordPuzzle player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerSwordPuzzle>();
    }

    private void LateUpdate()
    {
        var heldSword = player ? player.HeldSword : sword;
        if (!heldSword || !heldSword.IsHeld) return;
        for (int i = 0; i < fingers.Length; i++)
            fingers[i].localRotation = closedRotations[i];
    }
}
