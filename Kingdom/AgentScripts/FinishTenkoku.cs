using System;
using System.Linq;
using GlowKeeper;
using Tenkoku.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FinishTenkoku
{
    public static object Main()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play mode first.");
        var module=UnityEngine.Object.FindAnyObjectByType<TenkokuModule>();
        var lib=module.GetComponentInChildren<TenkokuLib>();
        var old=UnityEngine.Object.FindAnyObjectByType<ForestDayNightCycle>();
        var schedule=module.GetComponent<TenkokuForestSchedule>();
        if(!schedule) schedule=module.gameObject.AddComponent<TenkokuForestSchedule>();
        schedule.sky=module;
        schedule.library=lib;
        schedule.moonlightGrading=old.nightColorGrading;
        schedule.daylightRays=old.daytimeRays;
        schedule.eclipseRenderer=lib.eclipseObject.GetComponent<Renderer>();
        old.enabled=false;
        old.sunAndMoon.enabled=false;
        old.nightColorGrading.weight=0;
        module.transform.position=Vector3.zero;
        module.setRotation=180;
        module.useAmbient=false;
        module.autoTime=false;
        module.useAutoTime=false;
        module.currentHour=18;
        module.currentMinute=30;
        module.currentSecond=0;
        lib.sunObject.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Sun URP");
        lib.renderObjectMoon.sharedMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Moon URP");
        foreach(var renderer in module.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if(renderer.sharedMaterial && renderer.sharedMaterial.shader.name == "TENKOKU/planet_shader")
            {
                renderer.sharedMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Celestial Particles URP");
                EditorUtility.SetDirty(renderer.sharedMaterial);
            }
        }
        module.weather_qualityCloud=1;
        lib.renderObjectCloudSphere.sharedMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Stable Clouds URP");
        EditorUtility.SetDirty(lib.renderObjectCloudSphere.sharedMaterial);
        if(schedule.eclipseRenderer) schedule.eclipseRenderer.enabled=false;
        foreach(var renderer in module.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;
        }
        RenderSettings.skybox=lib.skyMaterialLegacy;
        RenderSettings.sun=lib.lightObjectWorld;
        RenderSettings.fog=true;
        RenderSettings.fogDensity=0.003f;
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(0.4f,0.45f,0.55f);
        RenderSettings.ambientEquatorColor=new Color(0.29f,0.32f,0.36f);
        RenderSettings.ambientGroundColor=new Color(0.15f,0.17f,0.15f);
        RenderSettings.fogColor=new Color(0.48f,0.43f,0.38f);
        RenderSettings.reflectionIntensity=0.8f;
        typeof(TenkokuModule).GetMethod("LateUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(module,null);
        typeof(TenkokuModule).GetMethod("LateUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(module,null);
        lib.sunlightObject.LookAt(lib.sunSphereObject);
        lib.worldlightObject.localRotation=lib.sunlightObject.localRotation;
        EditorUtility.SetDirty(schedule);
        EditorUtility.SetDirty(module);
        EditorUtility.SetDirty(lib.sunObject.GetComponent<Renderer>().sharedMaterial);
        EditorUtility.SetDirty(lib.renderObjectMoon.sharedMaterial);
        AssetDatabase.SaveAssets();
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=module.gameObject;
        return new { loopMinutes=schedule.LoopSeconds/60, sunsetMinutes=schedule.sunsetMinutes, nightMinutes=schedule.nightMinutes, sunriseMinutes=schedule.sunriseMinutes, daylightMinutes=schedule.daylightMinutes, oldCycleEnabled=old.enabled, sunShader=lib.sunObject.GetComponent<Renderer>().sharedMaterial.shader.name, moonShader=lib.renderObjectMoon.sharedMaterial.shader.name };
    }
}
