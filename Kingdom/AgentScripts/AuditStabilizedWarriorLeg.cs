using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var player=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var source=player.animator;var controller=(AnimatorController)source.runtimeAnimatorController;var report=new StringBuilder();var csv=new StringBuilder("state,footIK,frame,side,hipX,hipY,hipZ,kneeX,kneeY,kneeZ,footX,footY,footZ,kneeFlex,kneeRotationStep,poleStep,poleRadius\n");
 report.AppendLine("Playing="+EditorApplication.isPlaying+" paused="+EditorApplication.isPaused+" controller="+controller.name+" valid human="+source.isHuman);
 foreach(var name in new[]{"Walking","Running"})
 {
 var state=controller.layers[0].stateMachine.states.Single(s=>s.state.name==name).state;var clip=(AnimationClip)state.motion;report.AppendLine(name+" source="+AssetDatabase.GetAssetPath(clip)+" duration="+clip.length+"s human="+clip.humanMotion+" FootIK="+state.iKOnFeet);
 foreach(bool footIK in new[]{false,true})
 {
 var go=UnityEngine.Object.Instantiate(source.gameObject);go.name="Temporary Leg Audit";go.hideFlags=HideFlags.HideAndDontSave;go.transform.SetParent(null);go.transform.position=Vector3.zero;go.transform.rotation=Quaternion.identity;go.transform.localScale=source.transform.lossyScale;foreach(var b in go.GetComponents<MonoBehaviour>())b.enabled=false;
 var a=go.GetComponent<Animator>();a.runtimeAnimatorController=null;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;a.stabilizeFeet=source.stabilizeFeet;var graph=PlayableGraph.Create("LegAudit");var play=AnimationClipPlayable.Create(graph,clip);play.SetApplyFootIK(footIK);play.SetApplyPlayableIK(false);var output=AnimationPlayableOutput.Create(graph,"Pose",a);output.SetSourcePlayable(play);graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);graph.Play();
 try{
 foreach(bool left in new[]{true,false})
 {
 var hip=a.GetBoneTransform(left?HumanBodyBones.LeftUpperLeg:HumanBodyBones.RightUpperLeg);var knee=a.GetBoneTransform(left?HumanBodyBones.LeftLowerLeg:HumanBodyBones.RightLowerLeg);var foot=a.GetBoneTransform(left?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot);
 Quaternion prev=Quaternion.identity;Vector3 prevPole=Vector3.zero;float maxStep=0,maxPole=0,minFlex=180,maxFlex=0,maxSide=0;int spikes=0;float prevRadius=0;
 const int count=180;
 for(int i=0;i<=count;i++)
 {
 play.SetTime(clip.length*i/count);graph.Evaluate(0);
 var h=go.transform.InverseTransformPoint(hip.position);var k=go.transform.InverseTransformPoint(knee.position);var f=go.transform.InverseTransformPoint(foot.position);float flex=180-Vector3.Angle(h-k,f-k);var axis=(f-h).normalized;var pole=k-(h+axis*Vector3.Dot(k-h,axis));float radius=pole.magnitude;float step=i==0?0:Quaternion.Angle(prev,knee.localRotation);float poleStep=i==0||radius<.015f||prevRadius<.015f?0:Vector3.Angle(prevPole,pole);maxStep=Mathf.Max(maxStep,step);maxPole=Mathf.Max(maxPole,poleStep);minFlex=Mathf.Min(minFlex,flex);maxFlex=Mathf.Max(maxFlex,flex);maxSide=Mathf.Max(maxSide,Mathf.Abs(pole.x));if(step>15)spikes++;
 csv.AppendLine(name+","+footIK+","+i+","+(left?"L":"R")+","+h.x+","+h.y+","+h.z+","+k.x+","+k.y+","+k.z+","+f.x+","+f.y+","+f.z+","+flex+","+step+","+poleStep+","+radius);
 prev=knee.localRotation;prevPole=pole;prevRadius=radius;
 }
 report.AppendLine("  IK="+footIK+" "+(left?"Left":"Right")+" kneeFlex="+minFlex.ToString("F2")+".."+maxFlex.ToString("F2")+" max rotation step="+maxStep.ToString("F2")+" max pole step="+maxPole.ToString("F2")+" side sway="+maxSide.ToString("F3")+" spikes="+spikes);
 }
 }finally{graph.Destroy();UnityEngine.Object.DestroyImmediate(go);}
 }
 }
 File.WriteAllText("AgentScripts/warrior-leg-audit-stabilized.txt",report.ToString());File.WriteAllText("AgentScripts/warrior-leg-audit-stabilized.csv",csv.ToString());result.Log(report.ToString());
 }
}

