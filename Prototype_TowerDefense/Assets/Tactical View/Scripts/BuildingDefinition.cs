using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Tactical Map/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    [Tooltip("Shown as the panel title")]
    public string displayName = "Barracks";

    [Tooltip("How many slots per row in the grid")]
    [Min(1)] public int columns = 6;

    [Tooltip("Slots, filled row by row (left to right, then the next row)")]
    public List<BuildingSlotData> slots = new List<BuildingSlotData>();
}

// One cell in the grid. A small struct like Landmark, so it can live in this file.
[System.Serializable]
public struct BuildingSlotData
{
    [Tooltip("Picture in the slot. Leave empty for an empty slot")]
    public Sprite icon;
    [Tooltip("Greyed out and not clickable")]
    public bool locked;
    [Tooltip("What happens when clicked, e.g. \"RecruitSwordsman\"")]
    public string actionId;
}
