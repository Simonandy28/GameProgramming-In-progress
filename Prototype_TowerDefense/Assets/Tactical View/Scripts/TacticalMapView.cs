using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TacticalMapView : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The calibrated MapDefinition asset")]
    public MapDefinition map;

    [Header("UI")]
    [Tooltip("The panel that gets shown/hidden (Root). Must NOT be the object this script is on.")]
    public GameObject root;

    [Header("Input")]
    [Tooltip("Player/ToggleMap (Tab)")]
    public InputActionReference toggleMap;

    [Header("Test (temporary)")]
    [Tooltip("Icons under Icons that follow their Target")]
    public MapIcon[] testIcons;

    [Header("Popups")]
    [Tooltip("MenuLayer: holds Blocker, ActionMenu and BuildingPanel, gets shown/hidden")]
    public GameObject menuLayer;

    [Header("Unit menu")]
    [Tooltip("The small dropdown with the buttons (for units)")]
    public RectTransform actionMenu;
    [Tooltip("Optional: shows the unit name")]
    public TMP_Text menuTitle;
    [Tooltip("Where the menu appears relative to the icon (screen pixels)")]
    public Vector2 menuOffset = new Vector2(20, -20);

    [Header("Building panel")]
    [Tooltip("The grid panel at the bottom (for buildings)")]
    public BuildingPanel buildingPanel;

    MapIcon selected;   // the icon whose menu/panel is open (null = none)

    public bool IsOpen => root.activeSelf;

    void OnEnable()
    {
        if (toggleMap == null) { Debug.LogError("TacticalMapView: assign the ToggleMap action.", this); enabled = false; return; }
        toggleMap.action.performed += OnToggle;   // call OnToggle when Tab is pressed
        toggleMap.action.Enable();                 // project-wide actions are already on, this is just a safety net
    }

    void OnDisable()
    {
        if (toggleMap == null) return;
        toggleMap.action.performed -= OnToggle;   // always unsubscribe what you subscribed
    }

    void Start()
    {
        root.SetActive(false);                     // map starts closed
        CloseActionMenu();                         // popups start closed too
        if (!map.IsValid) Debug.LogWarning("TacticalMapView: MapDefinition has no valid calibration yet.", this);
    }

    void OnToggle(InputAction.CallbackContext ctx) => SetOpen(!IsOpen);

    public void SetOpen(bool open)
    {
        root.SetActive(open);
        if (!open) CloseActionMenu();              // closing the map also closes all popups
    }

    // LateUpdate: runs after all Update()s, so units have already moved this frame
    void LateUpdate()
    {
        if (!IsOpen) return;                       // don't do any work while the map is hidden

        foreach (MapIcon icon in testIcons)
            if (icon && icon.target) PlaceIcon((RectTransform)icon.transform, icon.target.position);

        if (selected && actionMenu.gameObject.activeSelf)   // the unit menu follows its icon
            actionMenu.position = selected.transform.position + (Vector3)menuOffset;
    }

    /// Puts a UI icon on the map at the spot matching a world position
    public void PlaceIcon(RectTransform icon, Vector3 worldPosition)
    {
        Vector2 uv = map.WorldToUV(worldPosition); // 0..1 across the whole PNG
        icon.anchorMin = uv;                       // pin the icon's anchor to that spot...
        icon.anchorMax = uv;
        icon.anchoredPosition = Vector2.zero;      // ...and sit exactly on it
    }

    // ---------- Clicking icons ----------

    /// Called by MapIcon when it gets clicked: buildings get the grid, units get the dropdown
    public void OnIconClicked(MapIcon icon)
    {
        if (icon.building) OpenBuildingPanel(icon);
        else OpenActionMenu(icon);
    }

    // Moves the outline from the old icon to the new one (null = nothing selected)
    void Select(MapIcon icon)
    {
        if (selected) selected.SetSelected(false);
        selected = icon;
        if (selected) selected.SetSelected(true);
    }

    public void OpenActionMenu(MapIcon icon)
    {
        Select(icon);
        menuLayer.SetActive(true);
        if (buildingPanel) buildingPanel.Hide();
        actionMenu.gameObject.SetActive(true);
        actionMenu.position = icon.transform.position + (Vector3)menuOffset; // Overlay canvas: position = screen pixels
        if (menuTitle) menuTitle.text = icon.target ? icon.target.name : icon.name;
    }

    public void OpenBuildingPanel(MapIcon icon)
    {
        Select(icon);
        menuLayer.SetActive(true);
        actionMenu.gameObject.SetActive(false);
        buildingPanel.Show(icon.building, this);
    }

    /// Closes the unit menu AND the building panel.
    /// Called by the Blocker button, by SetOpen(false) and after a unit action.
    public void CloseActionMenu()
    {
        Select(null);
        if (menuLayer) menuLayer.SetActive(false);
    }

    // ---------- Results ----------

    /// Called by the 4 unit-menu buttons, each passes its own word ("Move", "Attack", ...)
    public void DoAction(string action)
    {
        if (!selected) return;
        string unit = selected.target ? selected.target.name : selected.name;
        Debug.Log($"{action} ordered for {unit}");
        CloseActionMenu();
    }

    /// Called by a BuildingSlot. The panel stays open so you can click several slots in a row.
    public void OnBuildingSlotClicked(BuildingSlotData slot)
    {
        if (!selected) return;
        Debug.Log($"{selected.building.displayName}: {slot.actionId}");
    }
}
