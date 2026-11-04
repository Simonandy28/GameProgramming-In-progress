using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;Application.runInBackground=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 var boots=a.GetComponentsInChildren<SkinnedMeshRenderer>().Single(r=>r.name=="A1_Boots");
 foreach(bool useScale in new[]{false,true}){var mesh=new UnityEngine.Mesh();boots.BakeMesh(mesh,useScale);var v=mesh.vertices.Select(x=>boots.transform.TransformPoint(x)).ToArray();result.Log("BOOTS useScale="+useScale+" meshbounds="+mesh.bounds+" rendererLocal="+boots.localBounds+" lossy="+boots.transform.lossyScale+" y-range="+v.Min(x=>x.y)+","+v.Max(x=>x.y)+" rootY="+p.transform.position.y+" grounded="+p.GetComponent<CharacterController>().isGrounded);UnityEngine.Object.DestroyImmediate(mesh);}
 result.Log("State Idle="+a.GetCurrentAnimatorStateInfo(0).IsName("Idle")+" modelLocal="+a.transform.localPosition);
 Capture("AgentScripts/warrior-armor-final.png",p);
 }
 static void Capture(string path,ThirdPersonAvatarController p){var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;float oldFov=camera.fieldOfView;camera.transform.position=p.transform.position+p.transform.forward*3.5f+p.transform.right*.5f+Vector3.up*1.1f;camera.transform.LookAt(p.transform.position+Vector3.up*.85f);camera.fieldOfView=42;var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1000,1000,24,RenderTextureFormat.ARGB32);Texture2D img=null;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;img=new Texture2D(1000,1000,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1000,1000),0,0);img.Apply();File.WriteAllBytes(path,img.EncodeToPNG());}finally{camera.targetTexture=previous;RenderTexture.active=active;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.fieldOfView=oldFov;UnityEngine.Object.DestroyImmediate(rt);if(img)UnityEngine.Object.DestroyImmediate(img);}}
}

