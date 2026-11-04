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
 if(EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new Exception("Editor must be stopped with compilation successful");
 var report=new StringBuilder();
 foreach(string path in new[]{"Assets/Glowkeeper_Update.unity","Assets/Scenes/GlowKeeper Forest.unity","Assets/_Recovery/0.unity","Assets/_Recovery/0 (1).unity"})
 {
 var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();if(all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))!=0)throw new Exception("Missing script in "+path);
 foreach(var c in all.Select(t=>t.GetComponent<TenkokuForestSchedule>()).Where(c=>c)){if(c.sunriseStartHour!=6||c.sunriseMinutes!=2||c.daylightMinutes!=12||c.sunsetMinutes!=3||c.nightMinutes!=30||c.LoopSeconds!=2820)throw new Exception("Cycle configuration mismatch in "+path);c.SetElapsedTime(0);EditorUtility.SetDirty(c);}
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);report.AppendLine("PASS "+path+": full 47-minute cycle saved; sunrise start, no missing scripts.");
 }
 EditorSceneManager.OpenScene("Assets/Glowkeeper_Update.unity",OpenSceneMode.Single);var player=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();if(!player||!player.animator||!player.animator.GetComponent<WarriorFootPlacement>())throw new Exception("Avatar configuration lost");
 AssetDatabase.SaveAssets();Application.runInBackground=PlayerSettings.runInBackground;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;File.WriteAllText("AgentScripts/full-day-cycle-final-check.txt",report.ToString());result.Log(report.ToString());
 }
}
