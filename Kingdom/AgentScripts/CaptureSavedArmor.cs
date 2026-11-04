using System;
using System.IO;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();Application.runInBackground=true;EditorApplication.isPaused=false;double time=Time.timeAsDouble;
 EditorApplication.CallbackFunction draw=null;draw=()=>{if(!EditorApplication.isPlaying||!p){EditorApplication.update-=draw;return;}if(Time.timeAsDouble-time<1.2)return;Capture("AgentScripts/warrior-armor-final.png",p);EditorApplication.update-=draw;};EditorApplication.update+=draw;result.Log("Queued final saved-outfit preview after foot placement settles.");
 } static void Capture(string path,ThirdPersonAvatarController p){var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;float oldFov=camera.fieldOfView;camera.transform.position=p.transform.position+p.transform.forward*3.5f+p.transform.right*.5f+Vector3.up*1.1f;camera.transform.LookAt(p.transform.position+Vector3.up*.85f);camera.fieldOfView=42;var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1000,1000,24,RenderTextureFormat.ARGB32);Texture2D img=null;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;img=new Texture2D(1000,1000,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1000,1000),0,0);img.Apply();File.WriteAllBytes(path,img.EncodeToPNG());}finally{camera.targetTexture=previous;RenderTexture.active=active;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.fieldOfView=oldFov;UnityEngine.Object.DestroyImmediate(rt);if(img)UnityEngine.Object.DestroyImmediate(img);}}
}


