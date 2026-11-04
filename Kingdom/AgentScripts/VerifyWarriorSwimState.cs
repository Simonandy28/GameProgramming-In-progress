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
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var start=p.transform.position;var cc=p.GetComponent<CharacterController>();
 var water=UnityEngine.Object.FindObjectsByType<BoxCollider>().Where(c=>c.CompareTag("Water")&&c.isTrigger).OrderByDescending(c=>c.bounds.size.x*c.bounds.size.z).First();
 cc.enabled=false;p.transform.position=water.bounds.center-Vector3.up*.25f;cc.enabled=true;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
 double then=Time.timeAsDouble;EditorApplication.CallbackFunction check=null;
 check=()=>{if(!EditorApplication.isPlaying||!p){EditorApplication.update-=check;return;}if(Time.timeAsDouble-then<1) return;
 var state=a.GetCurrentAnimatorStateInfo(0);
 File.AppendAllText("AgentScripts/axe-warrior-playcheck.txt",(state.IsName("Swimming")?"PASS ":"FAIL ")+"Actual Swimming state reached after transition; humanoid="+a.isHuman+", staff source="+p.GetComponentsInChildren<UnityEngine.Animations.ParentConstraint>().First().GetSource(0).sourceTransform.name+"\n");
 Capture("AgentScripts/axe-warrior-swimming.png");InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());cc.enabled=false;p.transform.position=start;cc.enabled=true;EditorApplication.update-=check;};EditorApplication.update+=check;
 result.Log("Checking the completed swimming-animation transition.");
 }
 static void Capture(string path){var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);Texture2D image=null;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}finally{camera.targetTexture=previous;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);if(image)UnityEngine.Object.DestroyImmediate(image);}}
}
