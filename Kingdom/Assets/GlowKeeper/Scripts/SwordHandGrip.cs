using UnityEngine;

// Close only the right-hand fingers while this sword is held.
public class SwordHandGrip : MonoBehaviour
{
    public FloatingSword sword;
    public Transform[] fingers;
    public Quaternion[] closedRotations;

    private PlayerSwordPuzzle player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerSwordPuzzle>();
    }

    private void LateUpdate()
    {
        var heldSword = player && player.sword ? player.sword : sword;
        if (!heldSword || !heldSword.IsHeld) return;
        for (int i = 0; i < fingers.Length; i++)
            fingers[i].localRotation = closedRotations[i];
    }
}
