using UnityEngine;
using UnityEngine.UI;

public class BuildingSlot : MonoBehaviour
{
    [SerializeField] Image icon;       // child "Icon"
    [SerializeField] Button button;    // Button on this object

    static readonly Color Grey = new Color(0.35f, 0.35f, 0.35f, 1f);

    BuildingSlotData data;
    TacticalMapView view;
    bool clickable;   // from the data: has a picture and isn't locked

    // Hook up the click by code, so every slot created from the prefab works without Inspector wiring
    void Awake() => button.onClick.AddListener(OnClick);

    /// Called by BuildingPanel right after creating this slot
    public void Setup(BuildingSlotData slotData, TacticalMapView mapView)
    {
        data = slotData;
        view = mapView;

        icon.enabled = data.icon != null;      // no picture -> empty cell
        icon.sprite = data.icon;
        clickable = data.icon != null && !data.locked;
        Refresh();
    }

    // Checked every frame, so the slot unlocks by itself as soon as the recruit is done
    void Update() => Refresh();

    void Refresh()
    {
        // Recruit slots are locked while their building is still busy with the previous recruit
        Recruiter recruiter = view ? view.SelectedRecruiter : null;
        bool busy = data.unitPrefab && recruiter && recruiter.IsBusy(data.unitPrefab);

        button.interactable = clickable && !busy;
        icon.color = (data.locked || busy) ? Grey : Color.white;   // grey when locked or busy
    }

    void OnClick() => view.OnBuildingSlotClicked(data);
}
