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
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 string line="Swim: IsSwimming="+p.animator.GetBool("IsSwimming")+", IsSwimMoving="+p.animator.GetBool("IsSwimMoving")+", time="+Time.time+", timeScale="+Time.timeScale;
 File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt",line+"\n");
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 if(!p.animator.GetBool("IsSwimming")||!p.animator.GetBool("IsSwimMoving"))throw new Exception("Swimming animation not reached");
 var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=JsonUtility.FromJson<Vector3>(SessionState.GetString("WorldCleanup.StartPosition","{}"));cc.enabled=true;
 result.Log(line);
 }
}
