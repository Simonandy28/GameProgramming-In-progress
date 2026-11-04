using System;
using System.IO;
using System.Linq;
using Tenkoku.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PrepareTenkoku
{
    public static object Main()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play mode first.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/GlowKeeper Forest.unity") throw new Exception("Open the forest scene.");
        Directory.CreateDirectory("SetupBackup/Tenkoku");
        EditorSceneManager.SaveScene(scene);
        if (!File.Exists("SetupBackup/Tenkoku/GlowKeeper Forest.unity")) File.Copy(scene.path, "SetupBackup/Tenkoku/GlowKeeper Forest.unity");
        var old = UnityEngine.Object.FindAnyObjectByType<GlowKeeper.ForestDayNightCycle>();
        if (old) { old.enabled = false; old.sunAndMoon.enabled = false; old.nightColorGrading.weight = 0; }
        var module = UnityEngine.Object.FindAnyObjectByType<TenkokuModule>();
        if (!module)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TENKOKU - DYNAMIC SKY/PREFABS/Tenkoku DynamicSky.prefab");
            module = ((GameObject)PrefabUtility.InstantiatePrefab(prefab)).GetComponent<TenkokuModule>();
        }
        module.enabled = false;
        module.useAutoFX = false;
        module.useTemporalAliasing = false;
        module.useSunRays = false;
        module.useSunFlare = false;
        module.disableMSA = false;
        module.enableFog = false;
        module.enableProbe = false;
        module.enableIBL = false;
        module.atmosphereModelTypeIndex = 0;
        module.useAtmosphereIndex = -1;
        module.useAmbient = true;
        module.ambientTypeIndex = 1;
        module.autoTime = false;
        module.useAutoTime = false;
        module.autoTimeSync = false;
        module.autoDateSync = false;
        module.currentYear = 2019;
        module.currentMonth = 3;
        module.currentDay = 21;
        module.currentHour = 18;
        module.currentMinute = 30;
        module.currentSecond = 0;
        module.setLatitude = 35;
        module.setLongitude = 0;
        module.setTZOffset = 0;
        module.enableDST = false;
        module.setRotation = 180;
        module.lightLayer = -1;
        module.allowMultiLights = false;
        module.nightBrightness = 0.5f;
        module.moonLightIntensity = 0.45f;
        module.weatherTypeIndex = 0;
        module.weather_setAuto = false;
        module.weather_cloudCumulusAmt = 0.2f;
        module.weather_cloudAltoStratusAmt = 0;
        module.weather_cloudCirrusAmt = 0;
        module.weather_OvercastAmt = 0;
        module.weather_RainAmt = 0;
        module.weather_SnowAmt = 0;
        module.weather_FogAmt = 0;
        module.weather_lightning = 0;
        module.auroraTypeIndex = 2;
        module.manualCamera = Camera.main.transform;
        module.mainCamera = Camera.main.transform;
        module.cameraTypeIndex = 1;
        Camera.main.farClipPlane = 1500;

        const string folder = "Assets/GlowKeeper/ForestSettings/Tenkoku";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/GlowKeeper/ForestSettings", "Tenkoku");
        foreach (var renderer in module.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (!materials[i]) continue;
                var destination = folder + "/" + materials[i].name + ".mat";
                if (!AssetDatabase.LoadAssetAtPath<Material>(destination)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(materials[i]), destination);
                materials[i] = AssetDatabase.LoadAssetAtPath<Material>(destination);
            }
            renderer.sharedMaterials = materials;
        }
        var lib = module.GetComponentInChildren<TenkokuLib>(true);
        foreach (var source in new[] { lib.skyMaterialLegacy, lib.skyMaterialElek })
        {
            var path = folder + "/" + source.name + ".mat";
            if (!AssetDatabase.LoadAssetAtPath<Material>(path)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), path);
        }
        lib.skyMaterialLegacy = AssetDatabase.LoadAssetAtPath<Material>(folder + "/" + lib.skyMaterialLegacy.name + ".mat");
        lib.skyMaterialElek = AssetDatabase.LoadAssetAtPath<Material>(folder + "/" + lib.skyMaterialElek.name + ".mat");
        RenderSettings.skybox = lib.skyMaterialLegacy;
        foreach (var camera in module.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
        foreach (var behaviour in module.GetComponentsInChildren<MonoBehaviour>(true))
        {
            var type = behaviour.GetType();
            if (type.Namespace == "Tenkoku.Effects" || behaviour is TenkokuCameraTools || type.Name.Contains("Temporal") || type.Name.Contains("VelocityBuffer")) behaviour.enabled = false;
        }
        lib.cloudPlaneObject.gameObject.SetActive(false);
        lib.renderObjectAurora.enabled = false;
        foreach (var light in module.GetComponentsInChildren<Light>(true))
        {
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.cullingMask = -1;
        }
        module.enabled = true;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = module.gameObject;
        return new { module.name, lib = lib.name, materials = module.GetComponentsInChildren<Renderer>(true).Select(r=> new { r.name, shader = r.sharedMaterial ? r.sharedMaterial.shader.name : "none" }).ToArray() };
    }
}
