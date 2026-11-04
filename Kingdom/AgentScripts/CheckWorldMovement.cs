using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Play stopped unexpectedly");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 var start=JsonUtility.FromJson<Vector3>(SessionState.GetString("WorldCleanup.StartPosition","{}"));
 float distance=Vector3.Distance(start,p.transform.position);
 string line="Walk: distance="+distance+", IsWalking="+p.animator.GetBool("IsWalking")+", time="+Time.time+", timeScale="+Time.timeScale+", sky elapsed="+UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>().ElapsedSeconds;
 File.WriteAllText("AgentScripts/world-cleanup-playcheck.txt",line+"\n");
 result.Log(line);
 if(distance<0.1f || !p.animator.GetBool("IsWalking"))throw new Exception("Walking did not reach movement and animation");
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W,Key.LeftShift));
 }
}
