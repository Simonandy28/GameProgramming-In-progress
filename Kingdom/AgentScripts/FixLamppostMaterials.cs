using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor.Rendering.Universal.ShaderGUI;
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        string[] names = { "Material black metal", "Material glass", "Material lamp" };
        foreach (var name in names)
        {
            var path = "Assets/Lamppost/Material/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) throw new Exception("Missing material: " + path);
            if (mat.shader.name == "Standard")
            {
                Undo.RecordObject(mat, "Convert lamppost material to URP");
                new StandardUpgrader("Standard").Upgrade(mat, MaterialUpgrader.UpgradeFlags.None);
                BaseShaderGUI.SetMaterialKeywords(mat, LitGUI.SetMaterialKeywords);
                EditorUtility.SetDirty(mat);
            }
            if (mat.shader.name != "Universal Render Pipeline/Lit" || !mat.shader.isSupported)
                throw new Exception("Unsupported lamppost shader: " + mat.shader.name);
            result.Log(name + ": " + mat.shader.name + ", texture=" + (mat.GetTexture("_BaseMap") ? mat.GetTexture("_BaseMap").name : "none") + ", surface=" + mat.GetFloat("_Surface") + ", alpha=" + mat.GetColor("_BaseColor").a + ", emission=" + mat.IsKeywordEnabled("_EMISSION") + ", queue=" + mat.renderQueue);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset("Assets/Lamppost/Prefab/Prefab Lamppost.prefab", ImportAssetOptions.ForceUpdate);
        result.Log("Saved all three shared materials; existing prefab references preserved.");
    }
}
