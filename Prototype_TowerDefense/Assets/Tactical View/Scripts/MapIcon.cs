using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Interfaces = "I want to receive these UI events". The EventSystem calls them for us.
[RequireComponent(typeof(Image))]
public class MapIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("The unit in the world this icon stands for")]
    public Transform target;

    [Tooltip("Set for buildings (opens the grid panel). Leave empty for units (opens the dropdown)")]
    public BuildingDefinition building;


    [Header("Sprites")]
    [Tooltip("Shown normally")]
    public Sprite normalSprite;
    [Tooltip("Same picture with outline: shown on hover and while the menu is open")]
    public Sprite activeSprite;

    TacticalMapView view;
    Image image;
    bool hovered;    // mouse is over the icon
    bool selected;   // this icon's menu is open

    void Awake()
    {
        view = GetComponentInParent<TacticalMapView>(true);  // the view on the Canvas above us
        image = GetComponent<Image>();                       // the Image on this same object
    }

    void OnEnable() => Refresh();

    // If the map closes while the mouse is on the icon, no Exit event arrives -> forget the hover here
    void OnDisable() { hovered = false; Refresh(); }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;
        view.OnIconClicked(this);
    }

    public void OnPointerEnter(PointerEventData e) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData e) { hovered = false; Refresh(); }

    /// Called by TacticalMapView when the menu opens/closes for this icon
    public void SetSelected(bool value) { selected = value; Refresh(); }

    // One place that decides which picture is shown
    void Refresh()
    {
        if (!image || !normalSprite || !activeSprite) return;  // sprites not assigned yet -> leave the Image alone
        image.sprite = (hovered || selected) ? activeSprite : normalSprite;
    }
}
