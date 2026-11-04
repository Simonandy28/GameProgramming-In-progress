using System;
using System.Linq;
using GlowKeeper;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class SetupDayNightCycle
{
    public static object Main()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/GlowKeeper Forest.unity" || scene.isDirty) throw new InvalidOperationException("Open the saved forest scene first.");
        const string settings = "Assets/GlowKeeper/ForestSettings/";
        var originalSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Idyllic Fantasy Nature/Materials/Skybox/Skybox.mat");
        var shader = Shader.Find("GlowKeeper/Forest Cycle Sky");
        if (!shader || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Cycle sky shader must compile first.");
        var sky = AssetDatabase.LoadAssetAtPath<Material>(settings + "DayNightSky.mat");
        if (!sky)
        {
            sky = new Material(shader) { name = "DayNightSky" };
            sky.SetTexture("_Tex", originalSky.GetTexture("_Tex"));
            sky.SetColor("_Tint", originalSky.GetColor("_Tint"));
            sky.SetFloat("_Exposure", originalSky.GetFloat("_Exposure"));
            sky.SetFloat("_Rotation", originalSky.GetFloat("_Rotation"));
            AssetDatabase.CreateAsset(sky, settings + "DayNightSky.mat");
        }
        sky.SetFloat("_NightBlend", 0);
        var cycle = UnityEngine.Object.FindAnyObjectByType<ForestDayNightCycle>();
        if (!cycle) cycle = new GameObject("Day Night Cycle").AddComponent<ForestDayNightCycle>();
        cycle.sunAndMoon = RenderSettings.sun ? RenderSettings.sun : UnityEngine.Object.FindObjectsByType<Light>().First(l => l.type == LightType.Directional);
        cycle.sunAndMoon.name = "Sun and Moon Light";
        cycle.sunAndMoon.lightmapBakeType = LightmapBakeType.Realtime;
        cycle.sunAndMoon.shadows = LightShadows.Soft;
        cycle.morningLightRotation = cycle.sunAndMoon.transform.eulerAngles;
        cycle.cycleSkybox = sky;
        cycle.daytimeRays = UnityEngine.Object.FindObjectsByType<ParticleSystemRenderer>().Where(r => r.name.Contains("GodRays")).Cast<Renderer>().ToArray();
        cycle.moonIntensity = 0.4f;
        cycle.nightSkyAmbient = new Color(0.16f, 0.21f, 0.33f);
        cycle.nightHorizonAmbient = new Color(0.1f, 0.14f, 0.23f);
        cycle.nightGroundAmbient = new Color(0.05f, 0.07f, 0.12f);

        // Put the visible moon above the lake in the starting camera view.
        if (Camera.main)
        {
            var direction = Camera.main.transform.forward;
            direction.y = 0;
            direction = direction.normalized * Mathf.Cos(22 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(22 * Mathf.Deg2Rad);
            cycle.moonLightRotation = Quaternion.LookRotation(-direction, Vector3.up).eulerAngles;
            sky.SetVector("_MoonDirection", direction);
        }

        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(settings + "MoonlightColorGrading.asset");
        if (!profile)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, settings + "MoonlightColorGrading.asset");
            var colors = profile.Add<ColorAdjustments>(true);
            colors.postExposure.Override(0.3f);
            colors.contrast.Override(0);
            colors.saturation.Override(-12);
            colors.colorFilter.Override(new Color(0.84f, 0.92f, 1));
            var balance = profile.Add<WhiteBalance>(true);
            balance.temperature.Override(-8);
            balance.tint.Override(0);
            var mixer = profile.Add<ChannelMixer>(true);
            mixer.redOutRedIn.Override(100);
            mixer.redOutGreenIn.Override(0);
            mixer.redOutBlueIn.Override(0);
            mixer.greenOutRedIn.Override(0);
            mixer.greenOutGreenIn.Override(100);
            mixer.greenOutBlueIn.Override(0);
            mixer.blueOutRedIn.Override(0);
            mixer.blueOutGreenIn.Override(0);
            mixer.blueOutBlueIn.Override(100);
            var split = profile.Add<SplitToning>(true);
            split.shadows.Override(new Color(0.5f, 0.5f, 0.5f));
            split.highlights.Override(new Color(0.5f, 0.5f, 0.5f));
            split.balance.Override(0);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.25f);
            bloom.threshold.Override(0.9f);
            bloom.tint.Override(new Color(0.65f, 0.8f, 1));
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.12f);
            foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(profile);
        }
        if (!cycle.nightColorGrading)
        {
            var volume = new GameObject("Moonlight Color Grading").AddComponent<Volume>();
            volume.transform.SetParent(cycle.transform);
            volume.isGlobal = true;
            volume.priority = 20;
            volume.sharedProfile = profile;
            volume.weight = 0;
            cycle.nightColorGrading = volume;
        }

        profile.TryGet<ColorAdjustments>(out var nightColors);
        nightColors.postExposure.Override(0);
        nightColors.colorFilter.Override(new Color(0.65f, 0.8f, 1));
        nightColors.saturation.Override(-20);
        profile.TryGet<ChannelMixer>(out var nightMixer);
        nightMixer.greenOutGreenIn.Override(85);
        nightMixer.blueOutGreenIn.Override(20);
        EditorUtility.SetDirty(nightColors);
        EditorUtility.SetDirty(nightMixer);
        EditorUtility.SetDirty(profile);

        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = cycle.morningSkyAmbient;
        RenderSettings.ambientEquatorColor = cycle.morningHorizonAmbient;
        RenderSettings.ambientGroundColor = cycle.morningGroundAmbient;
        EditorUtility.SetDirty(cycle);
        EditorUtility.SetDirty(sky);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = cycle.gameObject;
        return new { scene = scene.path, fade = cycle.fadeToNightSeconds, night = cycle.nightSeconds, sunrise = cycle.sunriseSeconds, loop = cycle.CycleDurationSeconds, rays = cycle.daytimeRays.Length };
    }
}
