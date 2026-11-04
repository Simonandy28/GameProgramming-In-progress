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
 var water=UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude).Where(c=>c.CompareTag("Water")&&c.isTrigger).OrderByDescending(c=>c.bounds.size.x*c.bounds.size.z).First();
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 var cc=p.GetComponent<CharacterController>(); cc.enabled=false;p.transform.position=water.bounds.center-Vector3.up*0.25f;cc.enabled=true;
 int frame=Time.frameCount; int updates=0;
 EditorApplication.CallbackFunction check=null;
 check=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=check;return;}
 if(Time.frameCount==frame)return;
 updates++;
 if(p.animator.GetBool("IsSwimming")){
 string line="Water trigger: swimming=True at "+p.transform.position+" after "+updates+" frames, time="+Time.time;
 File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt",line+"\n");
 EditorApplication.update-=check;
 } else if(updates>=60){File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt","Water trigger check failed; position="+p.transform.position+"\n");EditorApplication.update-=check;}
 };
 EditorApplication.update+=check;
 result.Log("Water-entry check scheduled for the next physics frames with input released.");
 }
}

