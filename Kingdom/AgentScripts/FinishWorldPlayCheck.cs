using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying||EditorApplication.isPaused)throw new Exception("Play stopped or paused unexpectedly");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 var sky=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
 float elapsed=Time.time-SessionState.GetFloat("WorldCleanup.CheckStart",0);
 if(elapsed<40||Time.timeScale!=1||!p.enabled||!p.animator.enabled||!sky.isActiveAndEnabled)throw new Exception("Runtime continuity check failed");
 string line="Continuity PASS: elapsed gameplay="+elapsed+"s, timeScale="+Time.timeScale+", player/Animator/camera enabled="+p.enabled+"/"+p.animator.enabled+"/"+UnityEngine.Object.FindAnyObjectByType<ThirdPersonCamera>().enabled+", sky elapsed="+sky.ElapsedSeconds+", phase="+sky.Phase;
 File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt",line+"\n");
 result.Log(line);
 foreach(var action in new[]{p.moveAction,p.runAction,p.jumpAction,p.descendAction})result.Log(action.action.name+": "+string.Join(", ",action.action.bindings.Select(b=>b.effectivePath)));
 var camera=Camera.main;
 var previous=camera.targetTexture;var active=RenderTexture.active;
 var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);
 Texture2D image=null;
 try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("AgentScripts/world-cleanup-preview.png",image.EncodeToPNG());}
 finally{camera.targetTexture=previous;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);if(image)UnityEngine.Object.DestroyImmediate(image);}
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
 Application.runInBackground=SessionState.GetBool("WorldCleanup.OriginalBackground",false);
 Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 EditorApplication.isPlaying=false;
 }
}
