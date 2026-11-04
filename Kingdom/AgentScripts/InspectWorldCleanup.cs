using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var s=new StringBuilder();
 s.AppendLine("Playing="+EditorApplication.isPlaying+" paused="+EditorApplication.isPaused+" scale="+Time.timeScale);
 for(int i=0;i<SceneManager.sceneCount;i++) { var scene=SceneManager.GetSceneAt(i); s.AppendLine("SCENE "+scene.path+" dirty="+scene.isDirty); foreach(var r in scene.GetRootGameObjects()) s.AppendLine("ROOT "+r.name); }
 foreach(var m in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))
 { if(m.GetType().Assembly.GetName().Name!="Assembly-CSharp" && !(m is Volume) && !m.GetType().FullName.Contains("VisualScripting"))continue;
 s.AppendLine("COMP "+Path(m.transform)+" "+m.GetType().FullName+" enabled="+m.enabled+" active="+m.gameObject.activeInHierarchy);
 if(m.GetType().Namespace=="GlowKeeper"||m.GetType().Name=="PlayerDeath"||m.GetType().Name=="ZombieAI"||m.GetType().Name=="ScriptMachine"||m is Volume) s.AppendLine(EditorJsonUtility.ToJson(m));
 }
 foreach(var a in UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsInactive.Include,FindObjectsSortMode.None))s.AppendLine("ANIM "+Path(a.transform)+" controller="+AssetDatabase.GetAssetPath(a.runtimeAnimatorController));
 System.IO.File.WriteAllText("AgentScripts/world-cleanup-inspection.txt",s.ToString());
 result.Log(s.ToString());
 }
 static string Path(Transform t){return t.parent==null?t.name:Path(t.parent)+"/"+t.name;}
}
