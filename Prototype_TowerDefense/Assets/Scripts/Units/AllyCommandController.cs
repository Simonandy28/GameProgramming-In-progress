using UnityEngine;

[RequireComponent(typeof(UnitBehavior))]
public class AllyCommandController : MonoBehaviour
{
    private UnitBehavior unitBehavior;

    private void Awake()
    {
        unitBehavior = GetComponent<UnitBehavior>();
    }

    // Call this from your player selection/click script
    public void CommandMoveTo(Vector3 targetPosition)
    {
        unitBehavior.SetMoveDestination(targetPosition);
    }
}