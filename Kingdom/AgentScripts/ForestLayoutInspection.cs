using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-shot inspection requested through a project-local marker.
[InitializeOnLoad]
public static class ForestLayoutInspection
{
    static ForestLayoutInspection() { EditorApplication.update += Check; }
    static void Check()
    {
        const string request = "AgentScripts/inspect-forest.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        File.Delete(request);
        try
        {
            var s = new StringBuilder();
            s.AppendLine("Scene: " + SceneManager.GetActiveScene().path + " Playing: " + EditorApplication.isPlaying);
            foreach (var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                s.AppendLine("Terrain " + t.name + " pos=" + t.transform.position + " size=" + t.terrainData.size);
                for (int z = 0; z <= 10; z++)
                {
                    for (int x = 0; x <= 10; x++)
                    {
                        var p = t.transform.position + Vector3.Scale(t.terrainData.size, new Vector3(x/10f,0,z/10f));
                        s.Append($"({p.x:F0},{p.z:F0})={t.SampleHeight(p)+t.transform.position.y:F1} ");
                    }
                    s.AppendLine();
                }
            }
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.Contains("House_") || t.name.Contains("Statue_") || t.name.Contains("FantasyHouse") || t.name.Contains("Road") || t.name.Contains("Hill")))
                s.AppendLine("Object " + t.name + " pos=" + t.position + " scale=" + t.lossyScale + " bounds=" + Bounds(t.gameObject));
            foreach (var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) s.AppendLine("Camera " + c.name + " pos="+c.transform.position+" rotation="+c.transform.eulerAngles);
            foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[]{"Assets/Hodaart", "Assets/KE Statues - Lite/Prefabs/URP", "Assets/Idyllic Fantasy Nature/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("House 0") || p.Contains("Concrete_") || p.Contains("Rock_Big")))
            {
                var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                s.AppendLine("Prefab " + path + " bounds=" + Bounds(go) + " scale=" + go.transform.localScale);
            }
            File.WriteAllText("AgentScripts/forest-inspection.txt",s.ToString());
        }
        catch(Exception e) { File.WriteAllText("AgentScripts/forest-inspection.txt",e.ToString()); }
    }
    static Bounds Bounds(GameObject go)
    {
        var rs=go.GetComponentsInChildren<Renderer>();
        if(rs.Length==0)return new Bounds();
        var b=rs[0].bounds;
        foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
        return b;
    }
}
