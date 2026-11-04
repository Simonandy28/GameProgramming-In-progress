using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlowKeeper;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public static class VerifyTenkokuVisibility
{
    // Keep every Unity API call on the Editor main thread, including delayed checks.
    public static Task<object> Main()
    {
        var c=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
        if(!Application.isPlaying || !c) throw new Exception("Enter Play mode first.");
        var result=new TaskCompletionSource<object>();
        float scale=Time.timeScale;
        bool background=Application.runInBackground;
        var camera=Camera.main;
        Quaternion rotation=camera.transform.rotation;
        var look=camera.GetComponent<IdyllicFantasyNature.CameraMovement>();
        bool lookWasEnabled=look && look.enabled;
        var moonChecks=new List<object>();
        var timestamps=new[]{180d,810d,1080d,1800d,1979d};
        Application.runInBackground=true;
        Time.timeScale=0;
        if(look) look.enabled=false;
        float heading=(float)typeof(TenkokuForestSchedule).GetField("forestHeading",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(c);
        camera.transform.rotation=Quaternion.Euler(0,heading,0);
        int index=0;
        int frame=Time.frameCount;
        c.SetElapsedTime(timestamps[0]);
        double deadline=EditorApplication.timeSinceStartup+.5;
        EditorApplication.CallbackFunction tick=null;
        Action cleanup=()=>
        {
            EditorApplication.update-=tick;
            camera.transform.rotation=rotation;
            if(look) look.enabled=lookWasEnabled;
            Time.timeScale=scale;
            Application.runInBackground=background;
            c.SetElapsedTime(0);
        };
        tick=()=>
        {
            try
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if(EditorApplication.timeSinceStartup<deadline || Time.frameCount<frame+2) return;
                if(index<timestamps.Length)
                {
                    var point=camera.WorldToViewportPoint(c.library.moonObject.position);
                    if(point.z<=0 || point.x<0 || point.x>1 || point.y<0 || point.y>1) throw new Exception("Moon outside starting view at "+timestamps[index]+": "+point);
                    if(!c.library.renderObjectMoon.enabled) throw new Exception("Moon renderer disabled.");
                    moonChecks.Add(new{seconds=timestamps[index],x=point.x,y=point.y,light=c.library.lightObjectNight.intensity});
                    index++;
                    c.SetElapsedTime(index<timestamps.Length ? timestamps[index] : 0);
                    deadline=EditorApplication.timeSinceStartup+.5;
                    frame=Time.frameCount;
                    return;
                }
                float sunset=Shader.GetGlobalFloat("_GlowKeeperSunsetAmount");
                if(sunset<.5f) throw new Exception("Orange sunset is not driven.");
                int decorative=c.daylightRays.Count(r=>r.enabled);
                if(decorative==0) throw new Exception("Decorative rays faded before sunset.");
                var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/GlowKeeper/ForestSettings/ForestRenderer.asset");
                var feature=renderer.rendererFeatures.OfType<ForestSunRaysFeature>().SingleOrDefault();
                if(!feature || !feature.isActive || !feature.raysShader) throw new Exception("URP rays feature missing.");
                if(ShaderUtil.GetShaderMessages(feature.raysShader).Any(m=>m.severity.ToString()=="Error")) throw new Exception("Sun ray shader failed compilation.");
                var report=new {moonChecks,sunsetAmount=sunset,decorativeRays=decorative,urpRaysFeature=feature.name,orangeSky=RenderSettings.skybox.shader.name,pausePassed=c.ElapsedSeconds==0};
                cleanup();
                result.TrySetResult(report);
            }
            catch(Exception ex) { cleanup(); result.TrySetException(ex); }
        };
        EditorApplication.update+=tick;
        return result.Task;
    }
}
