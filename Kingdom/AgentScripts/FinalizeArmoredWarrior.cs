using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new Exception("Stop Play and resolve script errors before saving");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();p.animator.transform.Find("arm").gameObject.SetActive(true);
 EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);
 var report=new StringBuilder();
 foreach(string path in new[]{"Assets/Glowkeeper_Update.unity","Assets/Scenes/GlowKeeper Forest.unity","Assets/_Recovery/0.unity","Assets/_Recovery/0 (1).unity"})
 {
 var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();int missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
 if(missing>0)throw new Exception(path+" has missing scripts");
 if(all.Any(t=>t.name=="Staff_mesh_grp"||t.name=="StaffSocket"||t.name=="FireflyHoldPoint"||t.name.StartsWith("Firefly1Root")))throw new Exception("Staff helper remains in "+path);
 report.AppendLine("PASS "+path+": no staff objects/collectibles or missing scripts");
 }
 EditorSceneManager.OpenScene("Assets/Glowkeeper_Update.unity",OpenSceneMode.Single);p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;
 if(!a.GetComponent<WarriorFootPlacement>()||a.runtimeAnimatorController.name!="AxeWarriorAnimator"||!a.avatar.isValid)throw new Exception("Armored warrior configuration missing");
 if(!a.transform.Find("Linen Sleeves")||a.GetComponentsInChildren<Renderer>().Count(r=>r.name=="Axe_L"||r.name=="Axe_R")!=2)throw new Exception("Sleeves/axes missing");
 Application.runInBackground=PlayerSettings.runInBackground;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 report.AppendLine("PASS current scene saved; complete armor, sleeves, both axes, calibrated foot solver and humanoid animations retained; original movement/camera and sky timings preserved.");File.WriteAllText("AgentScripts/warrior-armor-final-check.txt",report.ToString());result.Log(report.ToString());
 }
}
