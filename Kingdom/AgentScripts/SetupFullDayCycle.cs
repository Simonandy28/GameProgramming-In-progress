using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new Exception("Editor must be stopped with successful compilation");
 var report=new StringBuilder();
 foreach(string path in new[]{"Assets/Glowkeeper_Update.unity","Assets/Scenes/GlowKeeper Forest.unity","Assets/_Recovery/0.unity","Assets/_Recovery/0 (1).unity"})
 {
 var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);var cycles=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<TenkokuForestSchedule>(true));
 foreach(var cycle in cycles){cycle.sunriseStartHour=6;cycle.sunriseMinutes=2;cycle.daylightMinutes=12;cycle.sunsetMinutes=3;cycle.nightMinutes=30;cycle.SetElapsedTime(0);EditorUtility.SetDirty(cycle);report.AppendLine(path+": sunrise at 06:00; morning/noon/afternoon daylight=12min, sunrise=2min, sunset=3min, night=30min. Loop="+cycle.LoopSeconds/60+"min.");}
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 EditorSceneManager.OpenScene("Assets/Glowkeeper_Update.unity",OpenSceneMode.Single);AssetDatabase.SaveAssets();File.WriteAllText("AgentScripts/full-day-cycle-setup.txt",report.ToString());result.Log(report.ToString());
 }
}

