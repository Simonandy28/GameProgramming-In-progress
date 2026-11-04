using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play mode before saving the revision.");
        var root=Object.FindAnyObjectByType<SuimonoUrpOcean>();
        foreach(var fx in root.GetComponentsInChildren<Suimono.Core.SuimonoModuleFX>(true))Undo.DestroyObjectImmediate(fx);
        var schedule=Object.FindAnyObjectByType<TenkokuForestSchedule>();
        Undo.RecordObject(schedule,"Balance sunset shafts");schedule.sunRaysStrength=.9f;EditorUtility.SetDirty(schedule);
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(schedule.sky,false);
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(schedule,true);
        Selection.activeGameObject=schedule.gameObject;
        File.WriteAllText("AgentScripts/forest-revision.pending","playcheck");
        File.Move("AgentScripts/forest-revision.pending","AgentScripts/forest-revision.request");
        result.Log("Removed unused Built-in Suimono FX manager and saved the final scene. Started rendered verification.");
    }
}
