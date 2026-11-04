using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class SetupForest
{
    public static object Main()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("An open scene has unsaved changes.");
        const string root = "Assets/GlowKeeper";
        const string settings = root + "/ForestSettings";
        const string scenePath = "Assets/Scenes/GlowKeeper Forest.unity";
        if (!AssetDatabase.IsValidFolder(root)) AssetDatabase.CreateFolder("Assets", "GlowKeeper");
        if (!AssetDatabase.IsValidFolder(settings)) AssetDatabase.CreateFolder(root, "ForestSettings");

        // Keep the package demo and the existing SampleScene available unchanged.
        CopyOnce("Assets/Idyllic Fantasy Nature/Demo/Demo.unity", scenePath);
        CopyOnce("Assets/Settings/PC_RPAsset.asset", settings + "/ForestPipeline.asset");
        CopyOnce("Assets/Settings/PC_Renderer.asset", settings + "/ForestRenderer.asset");
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(settings + "/ForestPipeline.asset");
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(settings + "/ForestRenderer.asset");
        var rendererData = new SerializedObject(renderer);
        rendererData.FindProperty("m_RenderingMode").intValue = 0; // Forward for the original custom lighting graphs.
        rendererData.ApplyModifiedPropertiesWithoutUndo();
        var pipelineData = new SerializedObject(pipeline);
        pipelineData.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue = renderer;
        pipelineData.ApplyModifiedPropertiesWithoutUndo();
        pipeline.supportsCameraDepthTexture = true;
        pipeline.supportsCameraOpaqueTexture = true;
        pipeline.msaaSampleCount = 4;
        pipeline.shadowDistance = 100;
        EditorUtility.SetDirty(pipeline);
        EditorUtility.SetDirty(renderer);
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.SetQualityLevel(Array.IndexOf(QualitySettings.names, "PC"), true);
        QualitySettings.renderPipeline = pipeline;
        PlayerSettings.colorSpace = ColorSpace.Gamma; // Palette authored by the package publisher.

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        foreach (var terrain in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
        {
            if (!terrain.terrainData) continue;
            var original = terrain.terrainData;
            var path = settings + "/" + original.name + ".asset";
            CopyOnce(AssetDatabase.GetAssetPath(original), path);
            terrain.terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            var collider = terrain.GetComponent<TerrainCollider>();
            if (collider) collider.terrainData = terrain.terrainData;
        }
        foreach (var volume in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
        {
            if (!volume.sharedProfile) continue;
            var path = settings + "/ForestPostProcessing.asset";
            CopyOnce(AssetDatabase.GetAssetPath(volume.sharedProfile), path);
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        }
        var camera = Camera.main;
        if (camera)
        {
            var data = camera.GetUniversalAdditionalCameraData();
            data.SetRenderer(0);
            data.renderPostProcessing = true;
            camera.farClipPlane = 650;
        }
        var player = UnityEngine.Object.FindFirstObjectByType<IdyllicFantasyNature.PlayerMovement>();
        if (player)
        {
            var playerData = new SerializedObject(player);
            playerData.FindProperty("_movementSpeed").floatValue = 6;
            playerData.FindProperty("_runMultiplier").floatValue = 1.8f;
            playerData.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var buildScenes = EditorBuildSettings.scenes.ToList();
        buildScenes.RemoveAll(s => s.path == scenePath);
        buildScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.SaveAssets();
        if (SceneView.lastActiveSceneView && camera)
        {
            var view = SceneView.lastActiveSceneView;
            view.sceneLighting = true;
            view.LookAt(camera.transform.position + camera.transform.forward * 20, camera.transform.rotation, 20);
        }
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        return new { scene = scene.path, camera = camera ? camera.transform.position.ToString() : "missing", terrains = Terrain.activeTerrains.Length, pipeline = pipeline.name, colorSpace = PlayerSettings.colorSpace.ToString(), roots = scene.rootCount };
    }

    private static void CopyOnce(string source, string destination)
    {
        if (AssetDatabase.LoadMainAssetAtPath(destination)) return;
        if (!AssetDatabase.CopyAsset(source, destination)) throw new InvalidOperationException("Could not copy " + source);
    }
}
