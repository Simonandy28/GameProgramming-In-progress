using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Enter Play before live gait comparison");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var cc=p.GetComponent<CharacterController>();var feet=a.GetComponent<WarriorFootPlacement>();var original=p.transform.position;var home=original+Vector3.up*40.2f;var platform=GameObject.CreatePrimitive(PrimitiveType.Cube);platform.name="Temporary Gait Audit Ground";platform.transform.position=original+Vector3.up*39.5f;platform.transform.localScale=new Vector3(200,1,200);Application.runInBackground=true;EditorApplication.isPaused=false;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;
 int mode=0,stage=0,frames=0;float maxSide=0,maxPole=0,minFlex=180,maxFlex=0;Vector3 priorPole=Vector3.zero;float priorRadius=0;double then=Time.timeAsDouble;int priorFrame=-1;var report=new StringBuilder();var csv=new StringBuilder("case,state,time,phase,flex,lateral,poleStep,radius\n");
 Action<Key[]> keys=k=>InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(k));Action place=()=>{cc.enabled=false;p.transform.position=home;p.transform.rotation=Quaternion.identity;cc.enabled=true;};
 Action reset=()=>{frames=0;maxSide=0;maxPole=0;minFlex=180;maxFlex=0;priorRadius=0;};
 Action<string> finish=name=>{report.AppendLine("case="+mode+" placement="+feet.enabled+" stabilize="+a.stabilizeFeet+" "+name+" frames="+frames+" flex="+minFlex+".."+maxFlex+" lateral="+maxSide+" poleStep="+maxPole);};
 EditorApplication.CallbackFunction tick=null;
 tick=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=tick;return;}if(Time.frameCount==priorFrame)return;priorFrame=Time.frameCount;
 try{
 double elapsed=Time.timeAsDouble-then;
 if(stage==0){keys(new Key[0]);place();feet.enabled=mode==0||mode==3;a.stabilizeFeet=mode==0||mode==1;stage=1;then=Time.timeAsDouble;reset();return;}
 if(stage==1){if(elapsed<.6)return;keys(new[]{Key.W});stage=2;then=Time.timeAsDouble;return;}
 if(stage==2||stage==3)
 {
 string expected=stage==2?"Walking":"Running";var state=a.GetCurrentAnimatorStateInfo(0);
 if(elapsed>.7&&state.IsName(expected)&&!a.IsInTransition(0))
 {
 var h=p.transform.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);var k=p.transform.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);var f=p.transform.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.LeftFoot).position);var axis=(f-h).normalized;var pole=k-(h+axis*Vector3.Dot(k-h,axis));float radius=pole.magnitude;float step=radius>.02f&&priorRadius>.02f?Vector3.Angle(pole,priorPole):0;float flex=180-Vector3.Angle(h-k,f-k);maxSide=Mathf.Max(maxSide,Mathf.Abs(pole.x));maxPole=Mathf.Max(maxPole,step);minFlex=Mathf.Min(minFlex,flex);maxFlex=Mathf.Max(maxFlex,flex);frames++;csv.AppendLine(mode+","+expected+","+Time.time+","+state.normalizedTime+","+flex+","+Mathf.Abs(pole.x)+","+step+","+radius);priorPole=pole;priorRadius=radius;
 }
 if(elapsed<(stage==2?4.3:3.2))return;
 finish(expected);reset();
 if(stage==2){keys(new[]{Key.W,Key.LeftShift});stage=3;then=Time.timeAsDouble;return;}
 keys(new Key[0]);if(++mode<4){stage=0;then=Time.timeAsDouble;return;}
 File.WriteAllText("AgentScripts/warrior-leg-live-matrix.txt",report.ToString());File.WriteAllText("AgentScripts/warrior-leg-live-matrix.csv",csv.ToString());feet.enabled=true;a.stabilizeFeet=true;EditorApplication.update-=tick;
 }
 }catch(Exception e){keys(new Key[0]);File.WriteAllText("AgentScripts/warrior-leg-live-matrix.txt",report+"\nERROR "+e);EditorApplication.update-=tick;}
 };
 EditorApplication.update+=tick;result.Log("Comparing four live combinations of standing foot placement and Animator foot stabilization over repeated walking/running cycles.");
 }
}

