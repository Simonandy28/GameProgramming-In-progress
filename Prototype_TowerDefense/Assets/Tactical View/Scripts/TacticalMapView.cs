using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

// IPointerClickHandler: clicks on the parchment (which has no click script itself) bubble up to here
public class TacticalMapView : MonoBehaviour, IPointerClickHandler
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

    [Header("Icons")]
    [Tooltip("Prefab every map icon is created from (Image + MapIcon)")]
    public MapIcon iconPrefab;
    [Tooltip("The Icons object inside MapFrame: new icons are created under it")]
    public RectTransform iconParent;

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

    MapIcon selected;                                      // the icon whose menu/panel is open (null = none)
    readonly List<MapIcon> icons = new List<MapIcon>();    // every icon currently on the map

    string pendingOrder;          // "Move" or "Attack" while we wait for the target click, null otherwise
    UnitBehavior orderedUnit;     // the unit that gets that order

    Behaviour[] heroScripts;        // hero look / move / attack: switched off while the map is open
    CursorLockMode cursorBefore;    // cursor state before the map opened


    public bool IsOpen => root.activeSelf;

    /// The Recruiter of the building whose panel is open (null if it can't recruit)
    public Recruiter SelectedRecruiter => selected && selected.target ? selected.target.GetComponent<Recruiter>() : null;


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

        // The hero's scripts, if there is a hero in the scene (missing ones are simply null)
        heroScripts = new Behaviour[]
        {
            FindAnyObjectByType<MouseMovement>(),
            FindAnyObjectByType<PlayerMovement>(),
            FindAnyObjectByType<PlayerAttack>()
        };

    }

    void OnToggle(InputAction.CallbackContext ctx) => SetOpen(!IsOpen);

    public void SetOpen(bool open)
    {
        root.SetActive(open);
        if (!open) CloseActionMenu();              // closing the map also closes all popups and cancels orders

        // The hero locks the mouse for looking around: free it while the map is open, restore it afterwards
        if (open) cursorBefore = Cursor.lockState;
        Cursor.lockState = open ? CursorLockMode.None : cursorBefore;
        Cursor.visible = open || cursorBefore != CursorLockMode.Locked;

        // No looking around, walking or sword swings while you click on the map
        foreach (Behaviour script in heroScripts)
            if (script) script.enabled = !open;
    }


    // LateUpdate: runs after all Update()s, so units have already moved this frame
    void LateUpdate()
    {
        if (!IsOpen) return;                       // don't do any work while the map is hidden

        icons.RemoveAll(icon => icon == null);     // forget icons whose unit was destroyed

        foreach (MapIcon icon in icons)
            if (icon.target) PlaceIcon((RectTransform)icon.transform, icon.target.position);

        if (!selected && (menuLayer.activeSelf || pendingOrder != null))   // the selected unit died -> close / cancel
            CloseActionMenu();

        if (selected && actionMenu.gameObject.activeSelf)   // the unit menu follows its icon
            actionMenu.position = selected.transform.position + (Vector3)menuOffset;
    }

    /// Called by MapTrackable: creates an icon for it under Icons
    public MapIcon AddIcon(MapTrackable trackable)
    {
        MapIcon icon = Instantiate(iconPrefab, iconParent);
        icon.name = trackable.name + " Icon";
        icon.Setup(trackable);
        icons.Add(icon);
        return icon;
    }

    /// Puts a UI icon on the map at the spot matching a world position
    public void PlaceIcon(RectTransform icon, Vector3 worldPosition)
    {
        Vector2 uv = map.WorldToUV(worldPosition); // 0..1 across the whole PNG
        icon.anchorMin = uv;                       // pin the icon's anchor to that spot...
        icon.anchorMax = uv;
        icon.anchoredPosition = Vector2.zero;      // ...and sit exactly on it
    }

    // ---------- Clicking icons and the map ----------

    /// Called by MapIcon when it gets clicked
    public void OnIconClicked(MapIcon icon)
    {
        if (pendingOrder != null) { GiveOrder(icon.target.position); return; }   // waiting for a target -> this icon is it

        UnitBehavior unit = icon.target ? icon.target.GetComponent<UnitBehavior>() : null;
        if (icon.building) OpenBuildingPanel(icon);                     // your buildings get the grid
        else if (unit && unit.CompareTag("Ally")) OpenActionMenu(icon); // only your own units get the dropdown
        // enemy units, enemy buildings, towers, hero: clicking does nothing (but they can still be Attack targets)
    }


    /// A click on the parchment itself (not on an icon)
    public void OnPointerClick(PointerEventData e)
    {
        if (pendingOrder == null) return;
        if (e.button == PointerEventData.InputButton.Right) { CloseActionMenu(); return; }   // right-click cancels
        if (e.button != PointerEventData.InputButton.Left) return;

        // Screen pixel -> point inside the Icons rectangle -> 0..1 across the map (the reverse of PlaceIcon) -> world
        RectTransformUtility.ScreenPointToLocalPointInRectangle(iconParent, e.position, null, out Vector2 local);
        Vector2 uv = Rect.PointToNormalized(iconParent.rect, local);
        GiveOrder(map.UVToWorld(uv));
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

    /// Closes the unit menu AND the building panel, and cancels a waiting order.
    /// Called by the Blocker button, by SetOpen(false), after an order and on right-click.
    public void CloseActionMenu()
    {
        Select(null);
        pendingOrder = null;
        orderedUnit = null;
        if (menuLayer) menuLayer.SetActive(false);
    }

    // ---------- Orders ----------

    /// Called by the Move / Attack buttons, each passes its own word
    public void DoAction(string action)
    {
        if (!selected) return;
        orderedUnit = selected.target ? selected.target.GetComponent<UnitBehavior>() : null;
        if (!orderedUnit) { CloseActionMenu(); return; }

        pendingOrder = action;
        menuLayer.SetActive(false);   // hide menu + Blocker so the next click reaches the map; the unit stays highlighted
        Debug.Log(action == "Move" ? "Click on the map where to go (right-click cancels)"
                                   : "Click on the icon to attack (right-click cancels)");
    }

    void GiveOrder(Vector3 worldPosition)
    {
        if (orderedUnit)   // the unit may have died while we were choosing
        {
            if (pendingOrder == "Move") orderedUnit.OrderMove(worldPosition);   // walk there, don't stop to fight
            else orderedUnit.SetMoveDestination(worldPosition);                // attack: walk there and fight what's there
            Debug.Log($"{pendingOrder} order for {orderedUnit.name}");
        }
        CloseActionMenu();
    }

    /// Called by a BuildingSlot. The panel stays open so you can click several slots in a row.
    public void OnBuildingSlotClicked(BuildingSlotData slot)
    {
        if (!selected) return;

        Recruiter recruiter = SelectedRecruiter;
        if (recruiter && slot.unitPrefab) recruiter.Recruit(slot);
        else Debug.Log($"{selected.building.displayName}: {slot.actionId}");
    }

}
