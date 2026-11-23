using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDefinition", menuName = "Tactical Map/Map Definition")]
public class MapDefinition : ScriptableObject
{
    [Header("Map Image")]
    [Tooltip("The parchment map sprite shown when pressing TAB")]
    public Sprite mapSprite;

    [Tooltip("Pixel size of the image the landmark positions were measured in (Paint)")]
    public Vector2 imageSize = new Vector2(1275, 1233);

    [Header("Calibration")]
    [Tooltip("Spots you can find both in the scene and on the map. At least 3, spread out, not all on one line.")]
    public List<Landmark> landmarks = new List<Landmark>();

    // Fit result:  pixel.x = a*x + b*z + e   |   pixel.y = c*x + d*z + f
    // Serialized (but hidden) so builds have the values too: OnValidate only runs in the editor.
    [SerializeField, HideInInspector] float a, b, c, d, e, f;
    [SerializeField, HideInInspector] bool isValid;

    public bool IsValid => isValid;

    // Runs in the editor whenever you change something on this asset
    void OnValidate() => Recalculate();

    public void Recalculate()
    {
        isValid = false;
        int n = landmarks.Count;
        if (n < 3) return; // still typing them in, no fit possible yet

        // 1. Averages
        float mx = 0, mz = 0, mpx = 0, mpy = 0;
        foreach (Landmark lm in landmarks)
        {
            mx += lm.world.x; mz += lm.world.y;   // world.y holds the world Z!
            mpx += lm.pixel.x; mpy += lm.pixel.y;
        }
        mx /= n; mz /= n; mpx /= n; mpy /= n;

        // 2 + 3. Sums of the centred values
        float sxx = 0, szz = 0, sxz = 0, sxp = 0, szp = 0, sxq = 0, szq = 0;
        foreach (Landmark lm in landmarks)
        {
            float x = lm.world.x - mx, z = lm.world.y - mz;
            float p = lm.pixel.x - mpx, q = lm.pixel.y - mpy;
            sxx += x * x; szz += z * z; sxz += x * z;
            sxp += x * p; szp += z * p;
            sxq += x * q; szq += z * q;
        }

        // 4. Points on one line (or all the same) -> can't solve
        float det = sxx * szz - sxz * sxz;
        if (det <= 1e-6f * sxx * szz)
        {
            Debug.LogWarning("MapDefinition: landmarks are (almost) on one line, spread them out.", this);
            return;
        }

        // 5. Coefficients
        a = (sxp * szz - szp * sxz) / det;
        b = (szp * sxx - sxp * sxz) / det;
        c = (sxq * szz - szq * sxz) / det;
        d = (szq * sxx - sxq * sxz) / det;
        e = mpx - a * mx - b * mz;
        f = mpy - c * mx - d * mz;
        isValid = true;

        // 6. Error per landmark (struct -> copy out, change, put back)
        for (int i = 0; i < n; i++)
        {
            Landmark lm = landmarks[i];
            lm.error = Vector2.Distance(WorldToPixel(new Vector3(lm.world.x, 0, lm.world.y)), lm.pixel);
            landmarks[i] = lm;
        }
    }

    /// World position -> pixel on the PNG (Paint coordinates, Y counts downward)
    public Vector2 WorldToPixel(Vector3 w)
    {
        return new Vector2(a * w.x + b * w.z + e, c * w.x + d * w.z + f);
    }

    /// World position -> 0..1 on the map, UI style (0,0 = bottom left)
    public Vector2 WorldToUV(Vector3 w)
    {
        Vector2 p = WorldToPixel(w);
        return new Vector2(p.x / imageSize.x, 1f - p.y / imageSize.y);
    }

    /// 0..1 on the map -> world position (Y = 0, sample the terrain height yourself later)
    public Vector3 UVToWorld(Vector2 uv)
    {
        float px = uv.x * imageSize.x;
        float py = (1f - uv.y) * imageSize.y;
        float dx = px - e, dy = py - f;
        float k = a * d - b * c;
        return new Vector3((d * dx - b * dy) / k, 0f, (-c * dx + a * dy) / k);
    }

    [ContextMenu("Test: lake centre")]
    void TestLakeCentre()
    {
        Debug.Log($"a={a:F3} b={b:F3} e={e:F1} | c={c:F3} d={d:F3} f={f:F1} | valid={isValid}");
        Debug.Log($"Lake centre (133.1, 168.7) -> pixel {WorldToPixel(new Vector3(133.1f, 0, 168.7f))}  (expected ~596, 482)");
    }
}
