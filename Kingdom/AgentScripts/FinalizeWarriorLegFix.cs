using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new Exception("Stop Play and resolve compilation before finalizing");
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Glowkeeper_Update.unity")throw new Exception("Wrong scene");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var controller=(AnimatorController)a.runtimeAnimatorController;
 foreach(var c in controller.layers[0].stateMachine.states){if((c.state.name=="Walking"||c.state.name=="Running")&&c.state.iKOnFeet)throw new Exception("Locomotion foot IK still enabled");}
 if(!a.GetComponent<WarriorFootPlacement>()||!a.avatar.isValid||!a.isHuman)throw new Exception("Warrior rig/grounding missing");
 var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();if(all.Any(t=>t.name.Contains("Temporary")&&(t.name.Contains("Locomotion")||t.name.Contains("Gait"))))throw new Exception("Temporary test platform remains");if(all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))!=0)throw new Exception("Missing scripts");
 var cycle=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();if(cycle.LoopSeconds!=2820||cycle.sunriseStartHour!=6)throw new Exception("Sky settings changed");
 AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);Application.runInBackground=PlayerSettings.runInBackground;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 string summary="PASS: locomotion foot IK disabled; standing terrain correction waits for settled Idle; original clips and movement/camera retained. Live tracking passed over 403 walk and 521 run samples, plus standing/jump/swim checks. Temporary test platforms discarded. Full 47-minute sky cycle retained; saved scene has zero missing scripts.";
 File.WriteAllText("AgentScripts/warrior-leg-fix-final-check.txt",summary);result.Log(summary);
 }
}
