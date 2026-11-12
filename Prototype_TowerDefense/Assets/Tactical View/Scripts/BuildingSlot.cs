using UnityEngine;
using UnityEngine.UI;

public class BuildingSlot : MonoBehaviour
{
    [SerializeField] Image icon;       // child "Icon"
    [SerializeField] Button button;    // Button on this object

    BuildingSlotData data;
    TacticalMapView view;

    // Hook up the click by code, so every slot created from the prefab works without Inspector wiring
    void Awake() => button.onClick.AddListener(OnClick);

    /// Called by BuildingPanel right after creating this slot
    public void Setup(BuildingSlotData slotData, TacticalMapView mapView)
    {
        data = slotData;
        view = mapView;

        icon.enabled = data.icon != null;      // no picture -> empty cell
        icon.sprite = data.icon;
        icon.color = data.locked ? new Color(0.35f, 0.35f, 0.35f, 1f) : Color.white;  // grey when locked
        button.interactable = data.icon != null && !data.locked;
    }

    void OnClick() => view.OnBuildingSlotClicked(data);
}
