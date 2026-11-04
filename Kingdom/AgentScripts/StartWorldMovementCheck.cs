using System;
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
 Application.runInBackground=true;
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 SessionState.SetFloat("WorldCleanup.CheckStart",Time.time);
 SessionState.SetString("WorldCleanup.StartPosition",JsonUtility.ToJson(p.transform.position));
 SessionState.SetFloat("WorldCleanup.SkyStart",(float)UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>().ElapsedSeconds);
 Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
 result.Log("Play running, time="+Time.time+" scale="+Time.timeScale+"; walk input queued from "+p.transform.position);
 }
}
