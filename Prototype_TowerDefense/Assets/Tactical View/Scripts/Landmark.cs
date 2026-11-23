using UnityEngine;

[System.Serializable]
public struct Landmark
{
    [Tooltip("e.g. 'Big building'")]
    public string name;

    [Tooltip("World position: X = world X, Y = world Z (not height!)")]
    public Vector2 world;

    [Tooltip("Pixel position on TacticalMap.png, measured in Paint (Y counts downward)")]
    public Vector2 pixel;

    [Tooltip("Calculated automatically: how many pixels the fit is off for this point")]
    public float error;
}
