using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var s=new StringBuilder();var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;
 s.AppendLine("OLD ANIM "+EditorJsonUtility.ToJson(a));
 foreach(var bone in new[]{HumanBodyBones.Hips,HumanBodyBones.Head,HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot,HumanBodyBones.RightHand,HumanBodyBones.LeftHand}){var t=a.GetBoneTransform(bone);s.AppendLine("OLD BONE "+bone+" pos="+p.transform.InverseTransformPoint(t.position)+" rotation="+(Quaternion.Inverse(p.transform.rotation)*t.rotation));}
 var staff=p.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Staff_mesh_grp");s.AppendLine("STAFF "+EditorJsonUtility.ToJson(staff));s.AppendLine("CONSTRAINT "+EditorJsonUtility.ToJson(staff.GetComponent<ParentConstraint>()));
 foreach(var path in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Axe Warrior/Prefab"}).Select(AssetDatabase.GUIDToAssetPath).Where(x=>!x.Contains("Ground"))){var g=AssetDatabase.LoadAssetAtPath<GameObject>(path);s.AppendLine("PREFAB "+path+" active meshes="+string.Join(",",g.GetComponentsInChildren<Renderer>().Select(r=>r.name)));}
 // Find references to any old-model transform or component, including attachments.
 foreach(var m in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)){
 var so=new SerializedObject(m);var it=so.GetIterator();while(it.Next(true)){if(it.propertyType!=SerializedPropertyType.ObjectReference||!it.objectReferenceValue)continue;var c=it.objectReferenceValue as Component;var go=it.objectReferenceValue as GameObject;var t=c?c.transform:go?go.transform:null;if(t&&t.IsChildOf(a.transform))s.AppendLine("REF "+m.name+" "+m.GetType().Name+"."+it.propertyPath+" -> "+t.name);}}
 System.IO.File.WriteAllText("AgentScripts/axe-warrior-attachments.txt",s.ToString());result.Log(s.ToString());
 }
}
