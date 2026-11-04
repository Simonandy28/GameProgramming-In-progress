using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed)throw new Exception("Scripts have compile errors or are still compiling.");
 if(SceneManager.GetActiveScene().path!="Assets/Glowkeeper_Update.unity")throw new Exception("Wrong active scene");
 var player=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 var camera=UnityEngine.Object.FindAnyObjectByType<ThirdPersonCamera>();
 var cycle=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
 if(!player||!player.animator||!player.cameraTransform||!player.moveAction||!player.jumpAction||!player.runAction||!player.descendAction)throw new Exception("Avatar references incomplete");
 if(!camera||!camera.target||!cycle||!cycle.sky||!cycle.library)throw new Exception("Camera/sky references incomplete");
 var all=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
 int missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
 if(missing>0)throw new Exception("Missing scripts: "+missing);
 result.Log("Compilation passed; no missing scene scripts. Avatar controls and Animator, camera target, sky references valid. Water volumes="+all.Count(t=>t.name.StartsWith("WaterVolume"))+" houses="+all.Count(t=>t.name.IndexOf("House",StringComparison.OrdinalIgnoreCase)>=0)+" statues="+all.Count(t=>t.name.IndexOf("Statue",StringComparison.OrdinalIgnoreCase)>=0)+". Cycle="+cycle.sunsetMinutes+","+cycle.nightMinutes+","+cycle.sunriseMinutes+","+cycle.daylightMinutes+" minutes.");
 SessionState.SetBool("WorldCleanup.OriginalBackground",Application.runInBackground);
 EditorApplication.isPlaying=true;
 }
}
