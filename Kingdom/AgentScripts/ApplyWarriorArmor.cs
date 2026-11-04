using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop Play before changing the saved character");
 const string backup="SetupBackup/WarriorArmor-2026-11-04";var report=new StringBuilder();
 foreach(string path in new[]{"Assets/Glowkeeper_Update.unity","Assets/Scenes/GlowKeeper Forest.unity","Assets/_Recovery/0.unity","Assets/_Recovery/0 (1).unity"})
 {
 string dest=backup+"/"+path;Directory.CreateDirectory(Path.GetDirectoryName(dest));if(!File.Exists(dest))File.Copy(path,dest);
 var scene=SceneManager.GetActiveScene().path==path?SceneManager.GetActiveScene():EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
 var objects=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
 foreach(var m in objects.Where(t=>t).SelectMany(t=>t.GetComponents<StaffLampDepositor>()).ToArray())UnityEngine.Object.DestroyImmediate(m);
 foreach(var m in objects.Where(t=>t).SelectMany(t=>t.GetComponents<StaffFireflyCollector>()).ToArray())UnityEngine.Object.DestroyImmediate(m);
 foreach(var lamp in objects.Where(t=>t).SelectMany(t=>t.GetComponents<LampReceiver>()).ToArray())
 {
 var destination=lamp.fireflyDestination;var magicLight=lamp.magicLampLight;
 UnityEngine.Object.DestroyImmediate(lamp);
 if(destination)UnityEngine.Object.DestroyImmediate(destination.gameObject);
 if(magicLight&&magicLight.name=="MagicLampLight")UnityEngine.Object.DestroyImmediate(magicLight.gameObject);
 }
 foreach(var t in objects.Where(t=>t).ToArray())
 {
 if(!t)continue;
 if(t.name=="Staff_mesh_grp"||t.name=="StaffSocket"||t.name.StartsWith("Firefly1Root"))
 {
 var prefabRoot=PrefabUtility.GetOutermostPrefabInstanceRoot(t.gameObject);if(prefabRoot)PrefabUtility.UnpackPrefabInstance(prefabRoot,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
 UnityEngine.Object.DestroyImmediate(t.gameObject);
 }
 }
 if(path=="Assets/Glowkeeper_Update.unity")
 {
 var player=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var model=player.animator.gameObject;
 var template=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Axe Warrior/Prefab/Axe_Warrior_1.prefab");
 foreach(var source in template.GetComponentsInChildren<Transform>(true))
 {if(source==template.transform)continue;string relative=AnimationUtility.CalculateTransformPath(source,template.transform);var target=model.transform.Find(relative);if(!target)throw new Exception("Armor part missing: "+relative);target.gameObject.SetActive(source.gameObject.activeSelf);}
 // The clothed legs and boots replace the bare body pieces, rather than overlapping them.
 foreach(string name in new[]{"Feet","shin","thigh","hips","hips_02"}){var part=model.transform.Find(name);if(part)part.gameObject.SetActive(false);}
 player.animator.stabilizeFeet=true;
 var controller=(AnimatorController)player.animator.runtimeAnimatorController;
 foreach(var layer in controller.layers)foreach(var state in layer.stateMachine.states)report.AppendLine("Existing foot IK "+state.state.name+"="+state.state.iKOnFeet);
 report.AppendLine("Armored active meshes: "+string.Join(",",model.GetComponentsInChildren<Renderer>().Select(r=>r.name)));
 }
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);report.AppendLine("Removed staff, collection/deposit scripts, collectible fly objects and lamp interaction helpers from "+path+"; scenery lamps retained.");
 }
 EditorSceneManager.OpenScene("Assets/Glowkeeper_Update.unity",OpenSceneMode.Single);AssetDatabase.SaveAssets();
 File.WriteAllText("AgentScripts/warrior-armor-changes.txt",report.ToString());result.Log(report.ToString());
 }
}
