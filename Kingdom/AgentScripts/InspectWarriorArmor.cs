using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var s=new StringBuilder();var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;
 s.AppendLine("Scene="+SceneManager.GetActiveScene().path+" playing="+EditorApplication.isPlaying+" dirty="+SceneManager.GetActiveScene().isDirty+" pos="+p.transform.position+" model local="+a.transform.localPosition+" scale="+a.transform.localScale);
 foreach(var m in p.GetComponents<MonoBehaviour>())s.AppendLine("PLAYER "+m.GetType().Name);
 foreach(var r in a.GetComponentsInChildren<Renderer>())s.AppendLine("ACTIVE MESH "+r.name+" bounds="+r.bounds);
 foreach(var bone in new[]{HumanBodyBones.Hips,HumanBodyBones.LeftUpperLeg,HumanBodyBones.LeftLowerLeg,HumanBodyBones.LeftFoot,HumanBodyBones.LeftToes,HumanBodyBones.RightUpperLeg,HumanBodyBones.RightLowerLeg,HumanBodyBones.RightFoot,HumanBodyBones.RightToes}){var t=a.GetBoneTransform(bone);s.AppendLine(bone+"="+t.name+" rootPos="+p.transform.InverseTransformPoint(t.position)+" localRot="+t.localEulerAngles);}
 var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/Axe Warrior/Model/Axe_Warrior.fbx");var h=importer.humanDescription;s.AppendLine("HUMANOID twist="+h.upperLegTwist+","+h.lowerLegTwist+" stretch="+h.legStretch+" spacing="+h.feetSpacing+" translation="+h.hasTranslationDoF);
 var armor=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Axe Warrior/Prefab/Axe_Warrior_1.prefab");foreach(var t in armor.GetComponentsInChildren<Transform>(true).Where(t=>t.GetComponent<Renderer>()))s.AppendLine("ARMOR "+t.name+" active="+t.gameObject.activeInHierarchy);
 var collect=p.GetComponent<StaffFireflyCollector>();if(collect){s.AppendLine("STAFF="+collect.fireflyHoldPoint.name);foreach(var f in collect.fireflies)s.AppendLine("COLLECT FLY "+(f.fireflyRoot?f.fireflyRoot.name:"null"));}
 foreach(var lamp in UnityEngine.Object.FindObjectsByType<LampReceiver>(FindObjectsInactive.Include))s.AppendLine("LAMP "+lamp.name+" light="+(lamp.magicLampLight?lamp.magicLampLight.name:"null")+" destination="+(lamp.fireflyDestination?lamp.fireflyDestination.name:"null"));
 File.WriteAllText("AgentScripts/warrior-armor-inspection.txt",s.ToString());result.Log(s.ToString());
 }
}
