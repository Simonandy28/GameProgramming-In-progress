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
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
 int firstFrame=Time.frameCount;
 EditorApplication.CallbackFunction check=null;
 check=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=check;return;}
 if(Time.frameCount<firstFrame+3)return;
 string line="Swim movement: IsSwimming="+p.animator.GetBool("IsSwimming")+", IsSwimMoving="+p.animator.GetBool("IsSwimMoving")+", time="+Time.time;
 File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt",line+"\n");
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=JsonUtility.FromJson<Vector3>(SessionState.GetString("WorldCleanup.StartPosition","{}"));cc.enabled=true;
 EditorApplication.update-=check;
 };
 EditorApplication.update+=check;
 result.Log("Swimming-animation check queued; it will release input and return the avatar to its starting point.");
 }
}
