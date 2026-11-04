using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 if(!p||!EditorApplication.isPlaying)throw new Exception("Play or avatar stopped");
 string line="Run: IsRunning="+p.animator.GetBool("IsRunning")+", controller enabled="+p.enabled+", time="+Time.time;
 File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt",line+"\n");
 if(!p.animator.GetBool("IsRunning"))throw new Exception("Running animation not reached");
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 var water=UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude).First(c=>c.CompareTag("Water"));
 SessionState.SetString("WorldCleanup.WaterCenter",JsonUtility.ToJson(water.bounds.center));
 var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=water.bounds.center;cc.enabled=true;
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
 result.Log(line+"; moved into water volume "+water.name+" to check swimming.");
 }
}
