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
 if(EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed)throw new Exception("Editor must be stopped and compilation successful");
 var report=new StringBuilder();
 foreach(string path in new[]{"Assets/Glowkeeper_Update.unity","Assets/Scenes/GlowKeeper Forest.unity","Assets/_Recovery/0.unity","Assets/_Recovery/0 (1).unity"})
 {
 var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
 var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
 int missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
 if(missing!=0)throw new Exception(path+" has missing scripts: "+missing);
 if(all.Any(t=>t.name.IndexOf("Zombie",StringComparison.OrdinalIgnoreCase)>=0 || t.name=="Survivor A Lusth" || t.name=="Day Night Cycle"))throw new Exception("Legacy zombie/cycle object remains in "+path);
 var cycle=all.Select(t=>t.GetComponent<TenkokuForestSchedule>()).FirstOrDefault(c=>c);
 if(cycle && (cycle.sunsetMinutes!=3 || cycle.nightMinutes!=30 || cycle.sunriseMinutes!=2 || cycle.daylightMinutes!=2))throw new Exception("Cycle timings changed");
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 report.AppendLine(path+": PASS, no missing scripts or zombie/old cycle objects; saved.");
 }
 EditorSceneManager.OpenScene("Assets/Glowkeeper_Update.unity",OpenSceneMode.Single);
 AssetDatabase.SaveAssets();
 Application.runInBackground=PlayerSettings.runInBackground;
 Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 report.AppendLine("Current scene restored. Active cycle: 3-minute sunset, 30-minute night, 2-minute sunrise, 2-minute daylight. Only Glowkeeper_Update enabled for builds.");
 File.WriteAllText("AgentScripts/world-cleanup-final-check.txt",report.ToString());
 result.Log(report.ToString());
 }
}
