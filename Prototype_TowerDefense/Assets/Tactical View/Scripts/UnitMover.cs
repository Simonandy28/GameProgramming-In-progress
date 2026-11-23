using System.Collections.Generic;
using UnityEngine;

/// Walks a unit along a route that PathNetwork finds.
public class UnitMover : MonoBehaviour
{
    [Tooltip("Leave empty to use the PathNetwork in the scene")]
    public PathNetwork network;
    [Tooltip("Walking speed in metres per second")]
    public float speed = 4f;
    [Tooltip("How fast the unit turns, in degrees per second")]
    public float turnSpeed = 540f;
    [Tooltip("Raise this if the unit's pivot is not at its feet (e.g. 1 for a default capsule)")]
    public float heightOffset = 0f;

    [Header("Test (temporary)")]
    public Transform testDestination;

    readonly List<Vector3> route = new List<Vector3>();
    int next;   // index of the waypoint we are walking towards

    public bool IsMoving => next < route.Count;

    void Awake()
    {
        if (!network) network = FindAnyObjectByType<PathNetwork>();
    }

    /// Finds a route to 'destination' and starts walking. Returns false if there is no route.
    public bool MoveTo(Vector3 destination)
    {
        if (!network.FindPath(transform.position, destination, route))
        {
            Stop();
            Debug.LogWarning($"{name}: no route to {destination}", this);
            return false;
        }
        next = 0;
        return true;
    }

    public void Stop()
    {
        route.Clear();
        next = 0;
    }

    void Update()
    {
        if (!IsMoving) return;

        // Walk on the flat ground plane (x/z only); the height comes from the terrain afterwards
        Vector3 pos = transform.position;
        Vector3 flatPos = new Vector3(pos.x, 0f, pos.z);
        Vector3 flatTarget = new Vector3(route[next].x, 0f, route[next].z);

        Vector3 newFlat = Vector3.MoveTowards(flatPos, flatTarget, speed * Time.deltaTime);
        Vector3 newPos = new Vector3(newFlat.x, 0f, newFlat.z);
        newPos.y = GroundHeight(newPos) + heightOffset;
        transform.position = newPos;

        // Turn smoothly towards the waypoint
        Vector3 dir = flatTarget - flatPos;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);

        // Waypoint reached -> next one (when the last is reached, IsMoving becomes false)
        if (newFlat == flatTarget) next++;
    }

    // SampleHeight is measured from the terrain's own position, so add that back
    float GroundHeight(Vector3 p) => network.terrain.SampleHeight(p) + network.terrain.transform.position.y;

    // ---------- Testing ----------

    [ContextMenu("Test: walk to destination")]
    void TestMove()
    {
        if (!Application.isPlaying) { Debug.LogWarning("UnitMover: units only walk in Play mode, press Play first.", this); return; }
        if (!testDestination) { Debug.LogWarning("UnitMover: assign Test Destination.", this); return; }
        MoveTo(testDestination.position);
    }

    // Remaining route, drawn while the unit is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 from = transform.position;
        for (int i = next; i < route.Count; i++)
        {
            Gizmos.DrawLine(from + Vector3.up, route[i] + Vector3.up);
            from = route[i];
        }
    }
}
