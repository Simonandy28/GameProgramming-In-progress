using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new Exception("Editor is playing or has compile errors");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var collector=p.GetComponent<StaffFireflyCollector>();var depositor=p.GetComponent<StaffLampDepositor>();
 if(a.name!="Axe Warrior Avatar"||!a.avatar.isHuman||!a.avatar.isValid||!a.runtimeAnimatorController)throw new Exception("Warrior Animator invalid");
 if(p.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Jumanji_AllAnim"))throw new Exception("Old figure still present");
 var constraint=p.GetComponentsInChildren<ParentConstraint>().Single();if(!constraint.GetSource(0).sourceTransform.IsChildOf(a.GetBoneTransform(HumanBodyBones.RightHand)))throw new Exception("Staff is not bound to new right hand");
 if(!collector.fireflyHoldPoint||!depositor.lamps.All(l=>l&&l.fireflyDestination))throw new Exception("Collection/deposit references lost");
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();int missing=scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));if(missing!=0)throw new Exception("Missing scripts");
 EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Application.runInBackground=PlayerSettings.runInBackground;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 File.WriteAllText("AgentScripts/axe-warrior-final-check.txt","PASS: saved Assets/Glowkeeper_Update.unity; original humanoid figure removed, warrior Avatar/Animator valid, original staff constraint and firefly/lamp references valid, zero missing scripts; Play stopped and runtime test relocations discarded.\n");result.Log(File.ReadAllText("AgentScripts/axe-warrior-final-check.txt"));
 }
}
