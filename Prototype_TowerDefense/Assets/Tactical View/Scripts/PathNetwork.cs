using System.Collections.Generic;
using UnityEngine;

/// Turns the painted path layer of a terrain into a walk grid and finds routes along it (A*).
public class PathNetwork : MonoBehaviour
{
    [Tooltip("The terrain with the painted paths (the land terrain)")]
    public Terrain terrain;
    [Tooltip("The terrain layer the paths are painted with (Dirt_Layer)")]
    public TerrainLayer pathLayer;
    [Tooltip("How strongly a spot must be painted to count as path (0..1)")]
    [Range(0.05f, 1f)] public float threshold = 0.35f;
    [Tooltip("How far (in cells, ~0.6 m each) to look for the nearest path when a point is off the path")]
    public int snapRadius = 80;

    [Header("Test (temporary)")]
    public Transform testFrom;
    public Transform testTo;

    bool[,] walkable;     // [x, z]: true = path
    int res;              // cells per side
    Vector3 origin;       // terrain corner in the world
    Vector2 cell;         // size of one cell in metres (x, z)
    readonly List<Vector3> lastPath = new List<Vector3>();   // drawn as a gizmo

    public bool IsBuilt => walkable != null;

    void Awake() => Build();

    // ---------- Building the grid ----------

    [ContextMenu("Build")]
    public void Build()
    {
        TerrainData data = terrain.terrainData;
        int layer = System.Array.IndexOf(data.terrainLayers, pathLayer);   // which paint channel is the path layer
        if (layer < 0) { Debug.LogError("PathNetwork: Path Layer is not used on this terrain.", this); return; }

        res = data.alphamapResolution;
        float[,,] maps = data.GetAlphamaps(0, 0, res, res);   // careful: [z, x, layer], row first!

        walkable = new bool[res, res];
        int count = 0;
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                if (maps[z, x, layer] >= threshold) { walkable[x, z] = true; count++; }

        origin = terrain.transform.position;
        cell = new Vector2(data.size.x / res, data.size.z / res);
        Debug.Log($"PathNetwork: {count} path cells of {res * res} ({cell.x:F2} m per cell)", this);
    }

    // ---------- Grid <-> world ----------

    Vector2Int WorldToCell(Vector3 p) => new Vector2Int(
        Mathf.Clamp(Mathf.FloorToInt((p.x - origin.x) / cell.x), 0, res - 1),
        Mathf.Clamp(Mathf.FloorToInt((p.z - origin.z) / cell.y), 0, res - 1));

    Vector3 CellToWorld(Vector2Int c)
    {
        Vector3 p = new Vector3(origin.x + (c.x + 0.5f) * cell.x, 0f, origin.z + (c.y + 0.5f) * cell.y);
        p.y = origin.y + terrain.SampleHeight(p);   // SampleHeight is relative to the terrain's position
        return p;
    }

    bool IsWalkable(int x, int z) => x >= 0 && z >= 0 && x < res && z < res && walkable[x, z];

    // Closest path cell to c: searches outward in growing rings
    bool FindNearestWalkable(Vector2Int c, out Vector2Int found)
    {
        found = c;
        if (IsWalkable(c.x, c.y)) return true;
        for (int r = 1; r <= snapRadius; r++)
        {
            float best = float.MaxValue;
            for (int dz = -r; dz <= r; dz++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;   // only the ring's border
                    if (!IsWalkable(c.x + dx, c.y + dz)) continue;
                    float d = dx * dx + dz * dz;
                    if (d < best) { best = d; found = new Vector2Int(c.x + dx, c.y + dz); }
                }
            if (best < float.MaxValue) return true;
        }
        return false;
    }

    // ---------- Route finding ----------

    /// Fills 'route' with world points from start to goal along the paths. Returns false if there is no route.
    public bool FindPath(Vector3 from, Vector3 to, List<Vector3> route)
    {
        route.Clear();
        if (!IsBuilt) Build();
        if (!FindNearestWalkable(WorldToCell(from), out Vector2Int start) ||
            !FindNearestWalkable(WorldToCell(to), out Vector2Int goal)) return false;

        int n = res * res;
        float[] g = new float[n];        // cheapest known cost from the start to each cell
        int[] cameFrom = new int[n];     // previous cell on that cheapest route
        bool[] closed = new bool[n];     // cell is finished
        for (int i = 0; i < n; i++) { g[i] = float.MaxValue; cameFrom[i] = -1; }

        // Cells waiting to be checked, sorted by "cost so far + estimated rest" -> always take the most promising
        var open = new SortedSet<(float f, int id)>();
        int startId = start.y * res + start.x, goalId = goal.y * res + goal.x;
        g[startId] = 0;
        open.Add((Heuristic(start, goal), startId));

        while (open.Count > 0)
        {
            var current = open.Min;
            open.Remove(current);
            int id = current.id;
            if (closed[id]) continue;
            if (id == goalId) break;
            closed[id] = true;

            int cx = id % res, cz = id / res;
            for (int dz = -1; dz <= 1; dz++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0) continue;
                    int nx = cx + dx, nz = cz + dz;
                    if (!IsWalkable(nx, nz)) continue;
                    bool diagonal = dx != 0 && dz != 0;
                    if (diagonal && (!IsWalkable(cx + dx, cz) || !IsWalkable(cx, cz + dz))) continue;   // no corner cutting

                    int nid = nz * res + nx;
                    float cost = g[id] + (diagonal ? 1.4142f : 1f);
                    if (cost >= g[nid]) continue;   // already know a cheaper way there
                    g[nid] = cost;
                    cameFrom[nid] = id;
                    open.Add((cost + Heuristic(new Vector2Int(nx, nz), goal), nid));
                }
        }
        if (goalId != startId && cameFrom[goalId] < 0) return false;   // goal never reached

        // Walk back from the goal along cameFrom, then flip the list
        var cells = new List<Vector2Int>();
        for (int c = goalId; c != -1; c = cameFrom[c]) cells.Add(new Vector2Int(c % res, c / res));
        cells.Reverse();

        // Smoothing: only keep a point where a straight line would leave the path
        int anchor = 0;
        route.Add(CellToWorld(cells[0]));
        for (int i = 2; i < cells.Count; i++)
            if (!LineOfSight(cells[anchor], cells[i]))
            {
                anchor = i - 1;
                route.Add(CellToWorld(cells[anchor]));
            }
        if (cells.Count > 1) route.Add(CellToWorld(cells[cells.Count - 1]));
        return true;
    }

    // Estimated remaining cost when moving in 8 directions (straight = 1, diagonal = 1.414)
    static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x), dz = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dz) + 0.4142f * Mathf.Min(dx, dz);
    }

    // True if every cell on the straight line from a to b is path
    bool LineOfSight(Vector2Int a, Vector2Int b)
    {
        Vector2 from = a, to = b;
        int steps = Mathf.CeilToInt(Vector2.Distance(from, to) * 2f);   // check every half cell
        for (int s = 1; s < steps; s++)
        {
            Vector2 p = Vector2.Lerp(from, to, s / (float)steps);
            if (!IsWalkable(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y))) return false;
        }
        return true;
    }

    // ---------- Testing ----------

    [ContextMenu("Test: find path")]
    void TestPath()
    {
        if (!testFrom || !testTo) { Debug.LogWarning("PathNetwork: assign Test From and Test To.", this); return; }
        Build();
        bool ok = FindPath(testFrom.position, testTo.position, lastPath);
        Debug.Log(ok ? $"PathNetwork: route with {lastPath.Count} waypoints" : "PathNetwork: no route found", this);
    }

    // Only drawn while this object is selected
    void OnDrawGizmosSelected()
    {
        if (walkable == null) return;

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);   // path cells (every 2nd one, keeps the editor fast)
        for (int z = 0; z < res; z += 2)
            for (int x = 0; x < res; x += 2)
                if (walkable[x, z])
                    Gizmos.DrawCube(CellToWorld(new Vector2Int(x, z)) + Vector3.up * 0.3f, new Vector3(cell.x, 0.1f, cell.y));

        Gizmos.color = Color.cyan;                        // last test route
        for (int i = 1; i < lastPath.Count; i++)
            Gizmos.DrawLine(lastPath[i - 1] + Vector3.up, lastPath[i] + Vector3.up);
    }
}

