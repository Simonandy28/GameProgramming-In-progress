using UnityEngine;

[RequireComponent(typeof(UnitBehavior))]
public class EnemyPathController : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private UnitBehavior unitBehavior;

    private void Awake()
    {
        unitBehavior = GetComponent<UnitBehavior>();
    }

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            unitBehavior.SetMoveDestination(waypoints[0].position);
        }
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // When the unit finishes walking to the current waypoint, advance to the next one
        if (unitBehavior.HasReachedDestination() && currentWaypointIndex < waypoints.Length - 1)
        {
            currentWaypointIndex++;
            unitBehavior.SetMoveDestination(waypoints[currentWaypointIndex].position);
        }
    }
}