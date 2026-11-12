using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanel : MonoBehaviour
{
    [SerializeField] TMP_Text title;          // optional
    [SerializeField] GridLayoutGroup grid;    // child "Grid", slots get created in here
    [SerializeField] BuildingSlot slotPrefab;

    public void Show(BuildingDefinition building, TacticalMapView view)
    {
        if (title) title.text = building.displayName;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = building.columns;

        // Remove the slots of the previously shown building
        for (int i = grid.transform.childCount - 1; i >= 0; i--)
        {
            GameObject old = grid.transform.GetChild(i).gameObject;
            old.SetActive(false);   // the layout ignores inactive children right away...
            Destroy(old);           // ...and the object is really removed at the end of the frame
        }

        // One slot per entry in the definition
        foreach (BuildingSlotData data in building.slots)
        {
            BuildingSlot slot = Instantiate(slotPrefab, grid.transform);
            slot.Setup(data, view);
        }

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
