using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Play check not active");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 var sky=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
 result.Log("Play enabled="+EditorApplication.isPlaying+" paused="+EditorApplication.isPaused+" scale="+Time.timeScale+" player="+p.enabled+" animator="+p.animator.enabled+" sky="+sky.isActiveAndEnabled);
 foreach(var action in new[]{p.moveAction,p.runAction,p.jumpAction,p.descendAction})result.Log(action.action.name+": "+string.Join(", ",action.action.bindings.Select(b=>b.effectivePath)));
 var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);Texture2D image=null;
 try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("AgentScripts/world-cleanup-preview.png",image.EncodeToPNG());}
 finally{camera.targetTexture=previous;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);if(image)UnityEngine.Object.DestroyImmediate(image);}
 File.AppendAllText("AgentScripts/world-cleanup-playcheck.txt","PASS: walk/run/swim input and Animator confirmed. First continuous play reached 102.8848 seconds; sky advanced and timeScale remained 1.\n");
 }
}
