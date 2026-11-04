using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using GlowKeeper;
using Tenkoku.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Project-local, one-shot authoring commands. No work happens without the request marker.
[InitializeOnLoad]
public static class ForestRevisionTools
{
    const string Request="AgentScripts/forest-revision.request";
    static ForestRevisionTools() { EditorApplication.update+=Check; File.WriteAllText("AgentScripts/forest-revision.ready","v5"); }
    static T[] All<T>() where T:Component => Resources.FindObjectsOfTypeAll<T>().Where(x=>x.gameObject.scene==SceneManager.GetActiveScene()).ToArray();
    static void Check()
    {
        if(SessionState.GetBool("ForestRevision.PlayCheck",false))TickPlayCheck();
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string command;
        try {command=File.ReadAllText(Request).Trim();} catch(IOException) {return;}
        File.Delete(Request);
        try
        {
            string result=command=="sky"?SetupSky():command=="inspect"?Inspect():command=="capture"?Capture():command=="verify"?Verify():command=="layout"?Layout():command=="moon"?MoonDebug():command=="water"?SetupWater():command=="playcheck"?StartPlayCheck():command=="finish"?Finish():"Unknown command: "+command;
            File.WriteAllText("AgentScripts/forest-revision-"+command+".txt",result);
        }
        catch(Exception ex) { File.WriteAllText("AgentScripts/forest-revision-"+command+".txt",ex.ToString()); Debug.LogException(ex); }
    }
    static void Save()
    {
        AssetDatabase.SaveAssets();
        var scene=SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        SceneView.RepaintAll();
    }
    static string SetupSky()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Scene edits require Edit mode.");
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/GlowKeeper Forest.unity")throw new Exception("Forest scene is not open.");
        EditorSceneManager.SaveScene(scene,"SetupBackup/ForestRevision/Forest-Before-Live-Edits.unity",true);
        var module=All<TenkokuModule>().Single();
        var schedule=All<TenkokuForestSchedule>().Single();
        var lib=All<TenkokuLib>().Single();
        Undo.RecordObjects(new Object[]{module,schedule,lib},"Restore native Tenkoku sky");
        foreach(var old in All<ForestDayNightCycle>())
        {
            Undo.RecordObject(old,"Disable custom sky cycle");
            old.enabled=false;
            if(old.sunAndMoon)old.sunAndMoon.enabled=false;
            if(schedule.daylightRays.Length==0)schedule.daylightRays=old.daytimeRays;
            if(!schedule.moonlightGrading)schedule.moonlightGrading=old.nightColorGrading;
            EditorUtility.SetDirty(old);
        }
        module.gameObject.SetActive(true);
        module.enabled=true;
        schedule.sky=module; schedule.library=lib;
        schedule.sunlightLimit=.8f; schedule.shadowStrength=.65f;
        schedule.nightSkyFill=new Color(.24f,.29f,.39f);
        schedule.nightHorizonFill=new Color(.23f,.25f,.30f);
        schedule.nightGroundFill=new Color(.23f,.20f,.17f);
        schedule.sunRaysStrength=1.15f;
        module.useAutoFX=false; module.useTemporalAliasing=false;
        module.useSunRays=false; module.useSunFlare=false;
        module.disableMSA=false; module.enableFog=false;
        module.enableProbe=false; module.enableIBL=false;
        module.useAmbient=false;
        module.atmosphereModelTypeIndex=0; module.useAtmosphereIndex=-1;
        module.moonTypeIndex=0;
        module.moonSize=.027f; module.moonDayIntensity=.3f;
        module.sunSize=.016f; module.sunBright=.7f; module.skyBrightness=.9f;
        module.manualCamera=Camera.main.transform; module.mainCamera=Camera.main.transform; module.cameraTypeIndex=1;
        lib.moonObject.localPosition=(Vector3)typeof(TenkokuModule).GetField("moonBasePosition",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(module);
        lib.moonSphereObject.localPosition=Vector3.zero;
        lib.renderObjectMoon.SetPropertyBlock(null);
        lib.renderObjectMoon.sharedMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Moon URP");
        lib.renderObjectMoon.sharedMaterial.SetFloat("_MoonBrightness",1.35f);
        lib.renderObjectMoon.sharedMaterial.SetColor("_PrimaryTint",Color.white);
        var sunMaterial=lib.sunObject.GetComponent<Renderer>().sharedMaterial;
        sunMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Sun URP");
        sunMaterial.SetFloat("_DiscBrightness",.65f);
        lib.skyMaterialLegacy.shader=Shader.Find("GlowKeeper/Tenkoku Orange Sunset URP");
        lib.renderObjectCloudSphere.sharedMaterial.shader=Shader.Find("GlowKeeper/Tenkoku Stable Clouds URP");
        foreach(var camera in module.GetComponentsInChildren<Camera>(true))camera.enabled=false;
        foreach(var effect in module.GetComponentsInChildren<MonoBehaviour>(true))
            if(effect.GetType().Namespace=="Tenkoku.Effects"||effect is TenkokuCameraTools||effect.GetType().Name.Contains("Temporal")||effect.GetType().Name.Contains("VelocityBuffer"))effect.enabled=false;
        lib.cloudPlaneObject.gameObject.SetActive(false);
        if(schedule.eclipseRenderer)schedule.eclipseRenderer.enabled=false;
        foreach(var light in module.GetComponentsInChildren<Light>(true)){light.lightmapBakeType=LightmapBakeType.Realtime;light.cullingMask=-1;}
        foreach(var r in module.GetComponentsInChildren<Renderer>(true)){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
        RenderSettings.skybox=lib.skyMaterialLegacy;
        RenderSettings.fogDensity=.0025f;
        schedule.enabled=false; schedule.enabled=true;
        Preview(0);
        var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/GlowKeeper/ForestSettings/ForestRenderer.asset");
        var rays=renderer.rendererFeatures.OfType<ForestSunRaysFeature>().Single();
        rays.raysShader=Shader.Find("Hidden/GlowKeeper/Forest Sun Rays");rays.SetActive(true);rays.Create();
        var ao=renderer.rendererFeatures.FirstOrDefault(f=>f.GetType().Name=="ScreenSpaceAmbientOcclusion");
        if(ao)
        {
            var so=new SerializedObject(ao);
            so.FindProperty("m_Settings.Intensity").floatValue=.22f;
            so.FindProperty("m_Settings.DirectLightingStrength").floatValue=.1f;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(ao);
        }
        renderer.SetDirty();
        foreach(var x in new Object[]{module,schedule,lib,lib.renderObjectMoon.sharedMaterial,sunMaterial,lib.skyMaterialLegacy,lib.renderObjectCloudSphere.sharedMaterial,renderer,rays})EditorUtility.SetDirty(x);
        Save(); Selection.activeGameObject=module.gameObject;
        return Verify();
    }
    static void Preview(double elapsed)
    {
        var c=All<TenkokuForestSchedule>().Single();
        c.SetElapsedTime(elapsed);
        var tick=typeof(TenkokuModule).GetMethod("LateUpdate",BindingFlags.NonPublic|BindingFlags.Instance);
        tick.Invoke(c.sky,null);tick.Invoke(c.sky,null);
        c.ApplyForestLighting();
    }
    static string Verify()
    {
        var c=All<TenkokuForestSchedule>().Single();
        var s=new StringBuilder();
        s.AppendLine("Tenkoku active: "+c.sky.isActiveAndEnabled+"; schedule active: "+c.isActiveAndEnabled);
        s.AppendLine("Custom cycles enabled: "+All<ForestDayNightCycle>().Count(x=>x.isActiveAndEnabled));
        s.AppendLine("Moon type (native=0): "+c.sky.moonTypeIndex+"; local position: "+c.library.moonObject.localPosition);
        s.AppendLine("Skybox: "+RenderSettings.skybox.shader.name);
        var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/GlowKeeper/ForestSettings/ForestRenderer.asset");
        s.AppendLine("Sun rays feature active: "+data.rendererFeatures.OfType<ForestSunRaysFeature>().Single().isActive);
        foreach(string name in new[]{"GlowKeeper/Tenkoku Moon URP","GlowKeeper/Tenkoku Sun URP","Hidden/GlowKeeper/Forest Sun Rays","GlowKeeper/Tenkoku Orange Sunset URP","GlowKeeper/Tenkoku Stable Clouds URP"})
        {
            var shader=Shader.Find(name);
            s.AppendLine(name+": "+(shader?"found":"MISSING"));
            if(shader)foreach(var msg in ShaderUtil.GetShaderMessages(shader))s.AppendLine("  "+msg.severity+" "+msg.message);
        }
        foreach(double time in new[]{0d,90d,180d,630d,1080d,1530d,1979d,1980d,2100d,2220d})
        {
            Preview(time);
            Vector3 native=c.library.moonObject.localPosition;
            Vector3 before=c.library.moonObject.position;
            Quaternion rotation=c.library.moonSphereObject.rotation;
            c.ApplyForestLighting();
            bool untouched=Vector3.Distance(before,c.library.moonObject.position)<.0001f&&Quaternion.Angle(rotation,c.library.moonSphereObject.rotation)<.001f;
            var moonDirection=(c.library.moonObject.position-c.sky.useCamera.position).normalized;
            s.AppendLine($"t={time}: {c.sky.currentHour:00}:{c.sky.currentMinute:00} sunY={-c.library.sunlightObject.forward.y:F3} moonElevation={Mathf.Asin(moonDirection.y)*Mathf.Rad2Deg:F1} nativeMoonUnchanged={untouched} moonLocal={native}");
        }
        Preview(0);
        return s.ToString();
    }
    static string Inspect()
    {
        var s=new StringBuilder();
        foreach(var b in All<MonoBehaviour>().Where(x=>x is TenkokuModule||x is TenkokuForestSchedule||x is ForestDayNightCycle||x.GetType().Namespace=="Suimono.Core"))s.AppendLine(b.GetType().Name+" "+b.name+" enabled="+b.enabled+" active="+b.gameObject.activeInHierarchy);
        foreach(var r in All<Renderer>().Where(x=>x.name.ToLowerInvariant().Contains("water")||x.name.ToLowerInvariant().Contains("ocean")))s.AppendLine("WaterRenderer "+r.name+" bounds="+r.bounds+" material="+string.Join(",",r.sharedMaterials.Select(m=>m?m.name+" ("+AssetDatabase.GetAssetPath(m)+")":"none")));
        foreach(var t in All<Terrain>())
        {
            s.AppendLine("Terrain "+t.name+" layers="+string.Join(",",t.terrainData.terrainLayers.Select(l=>l?l.name:"none")));
            if(t.name!="LandTerrain")continue;
            for(int z=115;z<=205;z+=5)
            {
                for(int x=50;x<=195;x+=5)
                {
                    var p=new Vector3(x,0,z);
                    float u=(x-t.transform.position.x)/t.terrainData.size.x,v=(z-t.transform.position.z)/t.terrainData.size.z;
                    var weights=t.terrainData.GetAlphamaps(Mathf.Clamp((int)(u*t.terrainData.alphamapWidth),0,t.terrainData.alphamapWidth-1),Mathf.Clamp((int)(v*t.terrainData.alphamapHeight),0,t.terrainData.alphamapHeight-1),1,1);
                    int dominant=0;for(int i=1;i<weights.GetLength(2);i++)if(weights[0,0,i]>weights[0,0,dominant])dominant=i;
                    s.Append($"{x},{z}:{t.SampleHeight(p)+t.transform.position.y:F1}/{t.terrainData.GetSteepness(u,v):F0}/{dominant} ");
                }
                s.AppendLine();
            }
        }
        foreach(var m in All<Renderer>().SelectMany(x=>x.sharedMaterials).Where(m=>m).Distinct().Where(m=>m.name.Contains("Bark")||m.name.Contains("Rock_")||m.name.Contains("Stone_")||m.name.ToLowerInvariant().Contains("road")))s.AppendLine("Material "+m.name+" path="+AssetDatabase.GetAssetPath(m)+" shader="+m.shader.name);
        return s.ToString();
    }
    static string Capture()
    {
        Preview(0);
        Render("Assets/Screenshots/Revision-Sunset.png",Camera.main.transform.position,Camera.main.transform.rotation);
        Render("Assets/Screenshots/Revision-Overview.png",new Vector3(150,115,232),Quaternion.LookRotation(new Vector3(125,12,150)-new Vector3(150,115,232)));
        Preview(1080);
        var c=All<TenkokuForestSchedule>().Single();
        Vector3 dir=(c.library.moonObject.position-Camera.main.transform.position).normalized;
        Render("Assets/Screenshots/Revision-Native-Moon.png",Camera.main.transform.position,Quaternion.LookRotation(dir));
        Preview(0);
        return "Saved sunset, overview and native moon previews.";
    }
    static string MoonDebug()
    {
        Preview(1080);
        var c=All<TenkokuForestSchedule>().Single();var lib=c.library;var r=lib.renderObjectMoon;
        var s=new StringBuilder();
        s.AppendLine("Moon renderer="+r.enabled+" active="+r.gameObject.activeInHierarchy+" layer="+r.gameObject.layer+" bounds="+r.bounds);
        s.AppendLine("Moon pos="+lib.moonObject.position+" scale="+lib.moonObject.lossyScale+" moon sphere pos="+lib.moonSphereObject.position+" sky="+lib.skyObject.position);
        s.AppendLine("Camera="+c.sky.useCamera.position+" far="+Camera.main.farClipPlane+" culling="+Camera.main.cullingMask);
        var m=r.sharedMaterial;
        s.AppendLine("Material="+m.name+" color="+m.GetColor("_Color")+" texture="+m.GetTexture("_MainTex")+" primary="+m.GetColor("_PrimaryTint"));
        s.AppendLine("SunDirection="+Shader.GetGlobalVector("Tenkoku_Vec_SunFwd")+" MoonLightColor="+Shader.GetGlobalColor("Tenkoku_MoonLightColor"));
        Render("Assets/Screenshots/Revision-Moon-Debug.png",Camera.main.transform.position,Quaternion.LookRotation(r.bounds.center-Camera.main.transform.position));
        foreach(var msg in ShaderUtil.GetShaderMessages(m.shader))s.AppendLine(msg.severity+" "+msg.message);
        Preview(0);return s.ToString();
    }
    static Bounds Bounds(GameObject go)
    {
        var rs=go.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;
        foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;
    }
    static string Layout()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Use Edit mode.");
        const string rootName="Hilltop Settlement Additions";
        if(All<Transform>().Any(t=>t.name==rootName))return "Layout already exists; no duplicates added.";
        var land=All<Terrain>().Single(t=>t.name=="LandTerrain");
        var root=new GameObject(rootName);Undo.RegisterCreatedObjectUndo(root,"Add hilltop settlement");
        var occupied=All<Transform>().Where(t=>t.name.StartsWith("House_")||t.name.StartsWith("Statue_")||t.name=="FantasyHouse_Centerpiece").Select(t=>t.position).ToList();
        var s=new StringBuilder();
        var houses=new[]{new Vector2(55,130),new Vector2(110,145),new Vector2(175,153),new Vector2(155,188)};
        for(int i=0;i<houses.Length;i++)
        {
            Vector3 p=FindGreenSite(land,houses[i],occupied,7,3);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Hodaart/HodaartLowPolyHouseCollection1/Prefabs/House {i+6:00}.prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root.transform);
            go.name=$"House_Hilltop_{i+6:00}";go.transform.localScale=Vector3.one*.5f;
            go.transform.rotation=Quaternion.Euler(0,new[]{110f,25f,240f,185f}[i],0);go.transform.position=p;
            var bounds=Bounds(go);go.transform.position+=Vector3.up*(p.y-bounds.min.y-.12f);
            occupied.Add(p);s.AppendLine(go.name+" at "+go.transform.position);
        }
        var sites=new[]{new Vector2(58,142),new Vector2(90,137),new Vector2(150,134),new Vector2(180,164),new Vector2(119,192),new Vector2(79,176)};
        for(int i=0;i<sites.Length;i++)
        {
            Vector3 p=FindGreenSite(land,sites[i],occupied,7,3.5f);
            var group=new GameObject($"Hilltop Shrine {i+1:00}");group.transform.SetParent(root.transform);group.transform.position=p;
            var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Idyllic Fantasy Nature/Prefabs/Rock_Big_{i%3+1:00}.prefab");
            var rock=(GameObject)PrefabUtility.InstantiatePrefab(rockPrefab,group.transform);
            rock.name=$"Hilltop_Stone_{i+1:00}";rock.transform.position=p;rock.transform.localScale=Vector3.one*.9f;rock.transform.rotation=Quaternion.Euler(0,35+i*53,0);
            var rb=Bounds(rock);rock.transform.position+=Vector3.up*(p.y-rb.min.y-.55f);
            // Raycast the actual rock mesh, rather than its bounding box, to support the statue.
            var colliders=rock.GetComponentsInChildren<MeshCollider>();
            if(colliders.Length==0)
            {
                foreach(var mf in rock.GetComponentsInChildren<MeshFilter>()) {var collider=mf.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=mf.sharedMesh;}
                colliders=rock.GetComponentsInChildren<MeshCollider>();
            }
            Physics.SyncTransforms();rb=Bounds(rock);
            var ray=new Ray(new Vector3(rb.center.x,rb.max.y+10,rb.center.z),Vector3.down);
            float top=float.NegativeInfinity;
            foreach(var collider in colliders)if(collider.Raycast(ray,out RaycastHit hit,30))top=Mathf.Max(top,hit.point.y);
            if(float.IsNegativeInfinity(top))throw new Exception("No rock support found for statue "+i);
            var statuePrefab=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/KE Statues - Lite/Prefabs/URP/Stylized/Concrete_Stylized_Statue_{(i%2==0?1:4)}.prefab");
            var statue=(GameObject)PrefabUtility.InstantiatePrefab(statuePrefab,group.transform);
            statue.name=$"Statue_Hilltop_Guardian_{i+1:00}";statue.transform.localScale=Vector3.one*2;
            statue.transform.SetPositionAndRotation(new Vector3(rb.center.x,top,rb.center.z),Quaternion.Euler(0,180+i*47,0));
            var sb=Bounds(statue);statue.transform.position+=Vector3.up*(top-sb.min.y-.025f);
            occupied.Add(p);s.AppendLine(statue.name+" on "+rock.name+" green hill at "+p+" rockTop="+top.ToString("F2"));
        }
        LightenSurfaces();Save();
        s.AppendLine("Added houses=4; statues=6; hilltop supporting rocks=6. Surface texture maps preserved.");
        return s.ToString();
    }
    static Vector3 FindGreenSite(Terrain land,Vector2 desired,System.Collections.Generic.List<Vector3> occupied,float clearance,float footprint)
    {
        float best=float.NegativeInfinity;Vector3 result=Vector3.zero;
        var trees=All<Transform>().Where(t=>t.name.StartsWith("Tree_")||t.name.StartsWith("Willow_")||t.name.StartsWith("Fir_")||t.name.StartsWith("Broadleaf_")).Select(t=>t.position).ToArray();
        for(float dz=-10;dz<=10;dz+=1.5f)for(float dx=-10;dx<=10;dx+=1.5f)
        {
            var p=new Vector3(desired.x+dx,0,desired.y+dz);
            float u=(p.x-land.transform.position.x)/land.terrainData.size.x,v=(p.z-land.transform.position.z)/land.terrainData.size.z;
            if(u<=0||v<=0||u>=1||v>=1)continue;
            var w=land.terrainData.GetAlphamaps((int)(u*(land.terrainData.alphamapWidth-1)),(int)(v*(land.terrainData.alphamapHeight-1)),1,1);
            if(w[0,0,0]<.6f)continue;
            float y=land.SampleHeight(p)+land.transform.position.y;
            float slope=land.terrainData.GetSteepness(u,v);
            if(y<10||slope>20)continue;
            if(occupied.Any(q=>Vector2.Distance(new Vector2(q.x,q.z),new Vector2(p.x,p.z))<clearance))continue;
            if(trees.Any(q=>Vector2.Distance(new Vector2(q.x,q.z),new Vector2(p.x,p.z))<footprint))continue;
            float variation=0;
            foreach(var offset in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back})variation=Mathf.Max(variation,Mathf.Abs(land.SampleHeight(p+offset*footprint)+land.transform.position.y-y));
            if(variation>1.25f)continue;
            float score=-(dx*dx+dz*dz)*.05f-slope*.1f-variation*2+y*.02f;
            if(score>best){best=score;result=new Vector3(p.x,y,p.z);}
        }
        if(float.IsNegativeInfinity(best))throw new Exception("No clear green site near "+desired);
        return result;
    }
    static void LightenSurfaces()
    {
        const string folder="Assets/GlowKeeper/ForestSettings/LightSurfaces";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/GlowKeeper/ForestSettings","LightSurfaces");
        var renderers=All<Renderer>();
        foreach(var source in renderers.SelectMany(r=>r.sharedMaterials).Where(m=>m).Distinct().ToArray())
        {
            bool bark=source.name.Contains("Bark");bool stone=source.name.StartsWith("Rock_")||source.name.StartsWith("Stone_")||source.name.StartsWith("Cliff_");
            if(!bark&&!stone)continue;
            string path=folder+"/"+source.name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source),path);material=AssetDatabase.LoadAssetAtPath<Material>(path);}
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",bark?new Color(.94f,.83f,.68f):new Color(.92f,.89f,.82f));
            if(material.HasProperty("_RockColor"))material.SetColor("_RockColor",bark?new Color(.92f,.80f,.63f,1):new Color(.83f,.80f,.72f,1));
            if(material.HasProperty("_Level"))material.SetFloat("_Level",.08f);
            if(material.HasProperty("_Contrast"))material.SetFloat("_Contrast",.18f);
            EditorUtility.SetDirty(material);
            foreach(var r in renderers)
            {
                var ms=r.sharedMaterials;bool changed=false;
                for(int i=0;i<ms.Length;i++)if(ms[i]==source){ms[i]=material;changed=true;}
                if(changed){Undo.RecordObject(r,"Lighten forest surfaces");r.sharedMaterials=ms;}
            }
        }
        foreach(var terrain in All<Terrain>().Where(t=>t.name=="LandTerrain"))
        {
            var layers=terrain.terrainData.terrainLayers;
            for(int i=0;i<layers.Length;i++)
            {
                var source=layers[i];if(!source||(!source.name.Contains("Dirt")&&!source.name.Contains("Cobblestone")))continue;
                string path=folder+"/"+source.name+".terrainlayer";
                if(!AssetDatabase.LoadAssetAtPath<TerrainLayer>(path))AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source),path);
                var layer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
                layer.diffuseRemapMin=new Vector4(.16f,.13f,.09f,0);layer.diffuseRemapMax=new Vector4(1,.95f,.86f,1);layer.normalScale=.7f;
                layers[i]=layer;EditorUtility.SetDirty(layer);
            }
            terrain.terrainData.terrainLayers=layers;EditorUtility.SetDirty(terrain.terrainData);
        }
    }
    static string SetupWater()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Use Edit mode.");
        if(All<SuimonoUrpOcean>().Length>0)return "Suimono ocean already installed.";
        const string folder="Assets/GlowKeeper/ForestSettings/Suimono";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/GlowKeeper/ForestSettings","Suimono");
        var root=new GameObject("Island Ocean - Suimono");root.SetActive(false);
        Undo.RegisterCreatedObjectUndo(root,"Install Suimono ocean");
        var moduleGO=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SUIMONO - WATER SYSTEM 2/PREFABS/SUIMONO_Module.prefab"),root.transform);
        var surfaceGO=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SUIMONO - WATER SYSTEM 2/PREFABS/SUIMONO_Surface.prefab"),root.transform);
        foreach(var b in root.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        foreach(var camera in root.GetComponentsInChildren<Camera>(true))camera.enabled=false;
        foreach(var r in root.GetComponentsInChildren<Renderer>(true))r.enabled=false;
        foreach(var p in root.GetComponentsInChildren<ParticleSystem>(true))p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        foreach(var light in root.GetComponentsInChildren<Light>(true))light.enabled=false;
        foreach(var collider in root.GetComponentsInChildren<Collider>(true))collider.enabled=false;
        foreach(var audio in root.GetComponentsInChildren<AudioSource>(true)){audio.playOnAwake=false;audio.enabled=false;}
        var module=moduleGO.GetComponent<Suimono.Core.SuimonoModule>();
        module.autoSetLayers=false;module.autoSetCameraFX=false;module.enableUnderwaterFX=false;module.enableDynamicReflections=false;module.enableCaustics=false;
        module.setCamera=Camera.main.transform;module.setTrack=Camera.main.transform;module.setLight=All<TenkokuForestSchedule>().Single().library.lightObjectWorld;
        var source=surfaceGO.GetComponent<Suimono.Core.SuimonoObject>();
        source.typeIndex=0;source.moduleObject=module;source.waveHeight=.35f;source.lgWaveHeight=.1f;source.lgWaveScale=.45f;source.turbulenceFactor=.6f;source.flowSpeed=.22f;source.flowDirection=225;
        source.shallowColor=new Color(.13f,.55f,.57f,1);source.depthColor=new Color(.035f,.18f,.27f,1);source.foamColor=new Color(.82f,.91f,.87f,1);source.enableFoam=true;
        source.enableUnderwater=false;source.enableTess=false;source.enableDynamicReflections=false;
        surfaceGO.name="Suimono Island Ocean Surface";surfaceGO.transform.SetPositionAndRotation(new Vector3(150,3,0),Quaternion.identity);surfaceGO.transform.localScale=Vector3.one;
        var surface=surfaceGO.transform.Find("Suimono_Object");surface.localPosition=Vector3.zero;surface.localRotation=Quaternion.identity;surface.localScale=Vector3.one;surface.gameObject.layer=4;
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"/OceanGrid.asset");
        if(!mesh)
        {
            const int n=384;const float width=2400;
            var vertices=new Vector3[(n+1)*(n+1)];var triangles=new int[n*n*6];
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)vertices[z*(n+1)+x]=new Vector3(((float)x/n-.5f)*width,0,((float)z/n-.5f)*width);
            int k=0;for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {int a=z*(n+1)+x;triangles[k++]=a;triangles[k++]=a+n+1;triangles[k++]=a+1;triangles[k++]=a+1;triangles[k++]=a+n+1;triangles[k++]=a+n+2;}
            mesh=new Mesh{name="Suimono Island Ocean Grid",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.bounds=new Bounds(Vector3.zero,new Vector3(width,4,width));
            AssetDatabase.CreateAsset(mesh,folder+"/OceanGrid.asset");
        }
        surface.GetComponent<MeshFilter>().sharedMesh=mesh;
        var material=new Material(Shader.Find("GlowKeeper/Suimono Ocean URP")){name="Suimono Island Ocean"};AssetDatabase.CreateAsset(material,folder+"/Ocean.mat");
        var renderer=surface.GetComponent<Renderer>();renderer.sharedMaterial=material;renderer.enabled=true;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=true;
        var adapter=root.AddComponent<SuimonoUrpOcean>();adapter.settings=source;adapter.library=moduleGO.GetComponent<Suimono.Core.SuimonoModuleLib>();adapter.oceanRenderer=renderer;adapter.oceanMaterial=material;
        foreach(var old in All<Renderer>().Where(r=>r.name=="Ocean"&&r!=renderer)){Undo.RecordObject(old,"Replace ocean surface");old.enabled=false;}
        foreach(var fx in All<Suimono.Core.Suimono_UnderwaterFog>())fx.enabled=false;
        root.SetActive(true);adapter.ApplySettings();
        Save();
        return "Installed Suimono ocean at sea level 3, covering 2400 x 2400 metres. Uses the imported Suimono prefab settings, three native wave textures, foam texture, and wave-height formula through a URP bridge. Legacy Built-in cameras and underwater FX disabled. Inland pond retained.";
    }
    static string StartPlayCheck()
    {
        SessionState.SetBool("ForestRevision.PlayCheck",true);EditorApplication.isPlaying=true;return "Started Play mode verification.";
    }
    static string Finish()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Use Edit mode.");
        var ocean=All<SuimonoUrpOcean>().Single();
        foreach(var fx in ocean.GetComponentsInChildren<Suimono.Core.SuimonoModuleFX>(true))Undo.DestroyObjectImmediate(fx);
        var c=All<TenkokuForestSchedule>().Single();c.sunRaysStrength=.9f;EditorUtility.SetDirty(c);
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(c.sky,false);
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(c,true);
        Selection.activeGameObject=c.gameObject;Save();
        return StartPlayCheck();
    }
    static int playStage=-1,playFrames;
    static bool oldBackground;
    static Quaternion playRotation;
    static StringBuilder playReport;
    static void TickPlayCheck()
    {
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            var c=All<TenkokuForestSchedule>().Single();
            if(playStage<0)
            {
                oldBackground=Application.runInBackground;Application.runInBackground=true;Time.timeScale=0;
                playRotation=Camera.main.transform.rotation;playReport=new StringBuilder();
                foreach(var look in All<IdyllicFantasyNature.CameraMovement>())look.enabled=false;
                c.SetElapsedTime(0);playStage=0;playFrames=0;
            }
            EditorApplication.QueuePlayerLoopUpdate();
            if(++playFrames<12)return;playFrames=0;
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/GlowKeeper/ForestSettings/ForestRenderer.asset");
            var feature=renderer.rendererFeatures.OfType<ForestSunRaysFeature>().Single();
            switch(playStage++)
            {
                case 0:
                    Render("Assets/Screenshots/Final-Tenkoku-Sunset.png",Camera.main.transform.position,playRotation);
                    feature.SetActive(false);break;
                case 1:
                    Render("Assets/Screenshots/Final-Tenkoku-Rays-Off.png",Camera.main.transform.position,playRotation);
                    feature.SetActive(true);c.SetElapsedTime(1080);break;
                case 2:
                    playReport.AppendLine("Runtime sky active="+c.sky.isActiveAndEnabled+" custom cycle active="+All<ForestDayNightCycle>().Any(x=>x.isActiveAndEnabled));
                    playReport.AppendLine("Native moon local="+c.library.moonObject.localPosition+" pos="+c.library.moonObject.position+" sky="+c.library.skyObject.position+" renderer="+c.library.renderObjectMoon.enabled);
                    Render("Assets/Screenshots/Final-Tenkoku-Night.png",Camera.main.transform.position,playRotation);
                    Camera.main.transform.rotation=Quaternion.LookRotation(c.library.renderObjectMoon.bounds.center-Camera.main.transform.position);break;
                case 3:
                    Render("Assets/Screenshots/Final-Tenkoku-Native-Moon.png",Camera.main.transform.position,Camera.main.transform.rotation);
                    Camera.main.transform.rotation=playRotation;c.SetElapsedTime(2100);break;
                case 4:
                    Render("Assets/Screenshots/Final-Tenkoku-Morning.png",Camera.main.transform.position,playRotation);
                    Camera.main.transform.position=new Vector3(170,22,55);
                    Camera.main.transform.rotation=Quaternion.LookRotation(new Vector3(150,3,-120)-Camera.main.transform.position);
                    c.SetElapsedTime(2160);break;
                case 5:
                    Render("Assets/Screenshots/Final-Suimono-Ocean.png",Camera.main.transform.position,Camera.main.transform.rotation);
                    var oceanShader=Shader.Find("GlowKeeper/Suimono Ocean URP");
                    foreach(var msg in ShaderUtil.GetShaderMessages(oceanShader))playReport.AppendLine("Ocean shader "+msg.severity+": "+msg.message);
                    playReport.AppendLine("Ocean bridges="+All<SuimonoUrpOcean>().Count(x=>x.isActiveAndEnabled));
                    playReport.AppendLine("Houses added="+All<Transform>().Count(x=>x.name.StartsWith("House_Hilltop_"))+" statues added="+All<Transform>().Count(x=>x.name.StartsWith("Statue_Hilltop_Guardian_")));
                    playReport.AppendLine(Verify());
                    File.WriteAllText("AgentScripts/forest-revision-playcheck-results.txt",playReport.ToString());
                    c.SetElapsedTime(0);Time.timeScale=1;Application.runInBackground=oldBackground;
                    SessionState.SetBool("ForestRevision.PlayCheck",false);playStage=-1;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception e)
        {
            File.WriteAllText("AgentScripts/forest-revision-playcheck-results.txt",e.ToString());SessionState.SetBool("ForestRevision.PlayCheck",false);Time.timeScale=1;EditorApplication.isPlaying=false;playStage=-1;
        }
    }
    public static void Render(string path,Vector3 position,Quaternion rotation)
    {
        var go=new GameObject("Temporary Forest Preview"){hideFlags=HideFlags.HideAndDontSave};
        var camera=go.AddComponent<Camera>();camera.CopyFrom(Camera.main);
        camera.enabled=false;camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=65;camera.aspect=16f/9f;
        var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.requiresDepthOption=CameraOverrideOption.On;data.requiresColorOption=CameraOverrideOption.On;
        var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
        Texture2D texture=null;
        try
        {
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
            File.WriteAllBytes(path,texture.EncodeToPNG());
        }
        finally {RenderTexture.active=previous;camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);if(texture)Object.DestroyImmediate(texture);Object.DestroyImmediate(go);}
    }
}
