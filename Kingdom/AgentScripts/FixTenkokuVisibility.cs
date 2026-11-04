using System;
using System.Linq;
using GlowKeeper;
using Tenkoku.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class FixTenkokuVisibility
{
    public static object Main()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play mode first.");
        var c=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
        c.frameMoonOverForest=true;
        c.moonMinimumElevation=16;
        c.moonMaximumElevation=24;
        c.sunsetOrangeStrength=1;
        c.sunRaysStrength=1.2f;
        c.sky.moonSize=.032f;
        c.library.skyMaterialLegacy.shader=Shader.Find("GlowKeeper/Tenkoku Orange Sunset URP");
        EditorUtility.SetDirty(c.library.skyMaterialLegacy);
        var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/GlowKeeper/ForestSettings/ForestRenderer.asset");
        var feature=renderer.rendererFeatures.OfType<ForestSunRaysFeature>().FirstOrDefault();
        if(!feature)
        {
            feature=ScriptableObject.CreateInstance<ForestSunRaysFeature>();
            feature.name="Forest Sunset Sun Rays";
            AssetDatabase.AddObjectToAsset(feature,renderer);
            renderer.rendererFeatures.Add(feature);
        }
        feature.raysShader=Shader.Find("Hidden/GlowKeeper/Forest Sun Rays");
        feature.SetActive(true);
        feature.Create();
        var rendererData=new SerializedObject(renderer);
        var featureMap=rendererData.FindProperty("m_RendererFeatureMap");
        featureMap.arraySize=renderer.rendererFeatures.Count;
        for(int i=0;i<renderer.rendererFeatures.Count;i++)
        {
            if(!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(renderer.rendererFeatures[i],out string guid,out long localId))
                throw new Exception("Could not persist renderer feature identity.");
            featureMap.GetArrayElementAtIndex(i).longValue=localId;
        }
        rendererData.ApplyModifiedPropertiesWithoutUndo();
        renderer.SetDirty();
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(feature);
        c.sky.currentHour=18;
        c.sky.currentMinute=30;
        c.sky.currentSecond=0;
        typeof(TenkokuModule).GetMethod("LateUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(c.sky,null);
        typeof(TenkokuModule).GetMethod("LateUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(c.sky,null);
        Shader.SetGlobalFloat("_GlowKeeperSunsetAmount",c.SunsetGlow*c.sunsetOrangeStrength);
        EditorUtility.SetDirty(c);
        EditorUtility.SetDirty(c.sky);
        AssetDatabase.SaveAssets();
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=c.gameObject;
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(c.sky,false);
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(c.sky.GetComponent<TenkokuCalculations>(),false);
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(c,true);
        return new { skyShader=c.library.skyMaterialLegacy.shader.name, moonSize=c.sky.moonSize, raysFeature=feature.name, c.frameMoonOverForest };
    }
}
