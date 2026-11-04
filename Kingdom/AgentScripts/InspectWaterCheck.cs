using System;
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
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 foreach(var c in UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude).Where(c=>c.CompareTag("Water")))result.Log(c.name+" enabled="+c.enabled+" trigger="+c.isTrigger+" center="+c.bounds.center+" size="+c.bounds.size);
 result.Log("Player pos="+p.transform.position+" swimming="+p.animator.GetBool("IsSwimming")+" time="+Time.time);
 var water=UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude).Where(c=>c.CompareTag("Water")&&c.isTrigger).OrderByDescending(c=>c.bounds.size.x*c.bounds.size.z).First();
 var cc=p.GetComponent<CharacterController>(); cc.enabled=false;p.transform.position=water.bounds.center;cc.enabled=true;
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Space));
 }
}
