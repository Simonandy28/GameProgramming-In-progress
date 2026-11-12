using UnityEngine;

/// Put this on anything that should appear on the tactical map (units, buildings).
/// It creates its own map icon when it appears and removes it when it is destroyed.
public class MapTrackable : MonoBehaviour
{
    [Tooltip("Icon picture")]
    public Sprite normalSprite;
    [Tooltip("Optional: same picture with outline, shown on hover / while the menu is open")]
    public Sprite activeSprite;
    [Tooltip("Only for buildings: clicking opens the grid panel instead of the unit dropdown")]
    public BuildingDefinition building;
    [Tooltip("Icon size on the map in pixels. The picture keeps its shape inside this square")]
    public float iconSize = 48f;


    MapIcon icon;

    // Start (not Awake): every Awake in the scene has run by then, so the map view is ready
    void Start()
    {
        TacticalMapView view = FindAnyObjectByType<TacticalMapView>();
        if (view) icon = view.AddIcon(this);
    }

    // Unit died / building removed -> its icon goes too
    void OnDestroy()
    {
        if (icon) Destroy(icon.gameObject);
    }
}
