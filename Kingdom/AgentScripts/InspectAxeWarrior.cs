using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var s=new StringBuilder();var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 s.AppendLine("Scene="+SceneManager.GetActiveScene().path+" dirty="+SceneManager.GetActiveScene().isDirty+" playing="+EditorApplication.isPlaying);
 foreach(var t in p.GetComponentsInChildren<Transform>(true))if(t==p.transform||t.parent==p.transform||t.GetComponent<Animator>()||t.GetComponent<Renderer>()||t.name.IndexOf("staff",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("hold",StringComparison.OrdinalIgnoreCase)>=0)s.AppendLine("PLAYER "+Path(t)+" local="+t.localPosition+" rot="+t.localEulerAngles+" scale="+t.localScale+" components="+string.Join(",",t.GetComponents<Component>().Select(c=>c?c.GetType().Name:"MISSING")));
 s.AppendLine("CC="+EditorJsonUtility.ToJson(p.GetComponent<CharacterController>()));
 foreach(var m in p.GetComponents<MonoBehaviour>())s.AppendLine("PLAYER SCRIPT "+m.GetType().Name+" "+EditorJsonUtility.ToJson(m));
 foreach(var a in p.GetComponentsInChildren<Animator>(true))Describe(a.gameObject,s);
 foreach(string path in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Axe Warrior"}).Select(AssetDatabase.GUIDToAssetPath)){s.AppendLine("PREFAB "+path);Describe(AssetDatabase.LoadAssetAtPath<GameObject>(path),s);}
 foreach(var asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Axe Warrior/Model/Axe_Warrior.fbx"))if(asset is Avatar)s.AppendLine("FBX AVATAR "+asset.name+" human="+((Avatar)asset).isHuman+" valid="+((Avatar)asset).isValid);
 System.IO.File.WriteAllText("AgentScripts/axe-warrior-inspection.txt",s.ToString()); result.Log(s.ToString());
 }
 static string Path(Transform t){return t.parent?Path(t.parent)+"/"+t.name:t.name;}
 static void Describe(GameObject g,StringBuilder s){foreach(var a in g.GetComponentsInChildren<Animator>(true))s.AppendLine("ANIM "+a.name+" avatar="+(a.avatar?a.avatar.name:"none")+" human="+(a.avatar&&a.avatar.isHuman)+" valid="+(a.avatar&&a.avatar.isValid)+" controller="+AssetDatabase.GetAssetPath(a.runtimeAnimatorController));foreach(var r in g.GetComponentsInChildren<Renderer>(true))s.AppendLine("RENDER "+Path(r.transform)+" bounds="+r.bounds+" materials="+string.Join(",",r.sharedMaterials.Select(m=>m?m.name+" ("+m.shader.name+")":"none")));}
}
