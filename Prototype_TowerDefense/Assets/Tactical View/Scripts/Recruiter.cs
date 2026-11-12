using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Put this on a building that recruits units (the barracks).
/// A recruited unit appears at the spawn point after its recruit time, then walks to the rally point.
/// Every unit type has its own timer: while a skeleton is being recruited, knights can still be recruited.
public class Recruiter : MonoBehaviour
{
    [Tooltip("Where new units appear, e.g. at the barracks door")]
    public Transform spawnPoint;
    [Tooltip("Where new units walk to. Put it ON or right next to the dirt path in front of the barracks")]
    public Transform rallyPoint;
    [Tooltip("Units stop at random spots within this radius around the rally point, so they don't all push for the same spot")]
    public float rallySpread = 2f;

    // Unit prefabs that are being recruited right now (a HashSet holds each entry at most once)
    readonly HashSet<GameObject> inProgress = new HashSet<GameObject>();

    /// True while this unit type is being recruited (its slot in the panel is locked meanwhile)
    public bool IsBusy(GameObject unitPrefab) => inProgress.Contains(unitPrefab);

    /// Called by the map when a slot with a unit prefab is clicked
    public void Recruit(BuildingSlotData slot)
    {
        if (IsBusy(slot.unitPrefab)) return;   // this unit type is still in progress

        inProgress.Add(slot.unitPrefab);
        Debug.Log($"{name}: recruiting {slot.unitPrefab.name}, ready in {slot.recruitTime} s", this);
        StartCoroutine(RecruitAfter(slot.unitPrefab, slot.recruitTime));
    }

    // A coroutine is a method that can pause (yield) and continue later.
    // Here: wait for the recruit time, then spawn the unit and send it off.
    IEnumerator RecruitAfter(GameObject prefab, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        GameObject unit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        Vector2 offset = Random.insideUnitCircle * rallySpread;   // random point in a circle
        Vector3 destination = rallyPoint.position + new Vector3(offset.x, 0f, offset.y);
        unit.GetComponent<UnitBehavior>().SetMoveDestination(destination);

        inProgress.Remove(prefab);   // this unit type can be recruited again
    }
}
