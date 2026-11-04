using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Enter Play before observing");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;double started=Time.timeAsDouble;Application.runInBackground=true;EditorApplication.isPaused=false;
 int walkFrames=0,runFrames=0;float walkSway=0,runSway=0,maxWalkPole=0,maxRunPole=0;Vector3 priorPole=Vector3.zero;float priorRadius=0;string lastState="";var csv=new StringBuilder("time,state,leftKneeX,leftKneeY,leftKneeZ,poleX,poleY,poleZ\n");
 EditorApplication.CallbackFunction observe=null;
 observe=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=observe;return;}
 var state=a.GetCurrentAnimatorStateInfo(0);string name=state.IsName("Walking")?"Walking":state.IsName("Running")?"Running":"";
 if(name!=""&&!a.IsInTransition(0))
 {
 var h=p.transform.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);var k=p.transform.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);var f=p.transform.InverseTransformPoint(a.GetBoneTransform(HumanBodyBones.LeftFoot).position);var axis=(f-h).normalized;var pole=k-(h+axis*Vector3.Dot(k-h,axis));float poleStep=lastState==name&&pole.magnitude>.015f&&priorRadius>.015f?Vector3.Angle(pole,priorPole):0;
 if(name=="Walking"){walkFrames++;walkSway=Mathf.Max(walkSway,Mathf.Abs(pole.x));maxWalkPole=Mathf.Max(maxWalkPole,poleStep);}else{runFrames++;runSway=Mathf.Max(runSway,Mathf.Abs(pole.x));maxRunPole=Mathf.Max(maxRunPole,poleStep);}
 csv.AppendLine(Time.time+","+name+","+k.x+","+k.y+","+k.z+","+pole.x+","+pole.y+","+pole.z);priorPole=pole;priorRadius=pole.magnitude;
 }
 else priorRadius=0;
 lastState=name;
 if(Time.timeAsDouble-started<17)return;
 bool passed=walkFrames>20&&runFrames>20&&walkSway<.055f&&runSway<.08f;
 File.WriteAllText("AgentScripts/warrior-leg-fix-live-motion.txt",(passed?"PASS ":"FAIL ")+"Live left-leg tracking across multiple strides: walking frames="+walkFrames+", max lateral knee bend="+walkSway+"m, max knee-plane frame change="+maxWalkPole+"deg; running frames="+runFrames+", max lateral knee bend="+runSway+"m, max knee-plane frame change="+maxRunPole+"deg.\n");File.WriteAllText("AgentScripts/warrior-leg-fix-live-motion.csv",csv.ToString());EditorApplication.update-=observe;
 };
 EditorApplication.update+=observe;result.Log("Tracking the live left knee across multiple walking/running strides.");
 }
}

