using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var placement=p.animator.GetComponent<WarriorFootPlacement>();Application.runInBackground=true;EditorApplication.isPaused=false;
 int stage=0;double then=Time.timeAsDouble;EditorApplication.CallbackFunction verify=null;
 verify=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=verify;return;}
 if(Time.timeAsDouble-then<1.5) return;
 if(stage==0){if(!p.GetComponent<CharacterController>().isGrounded||p.animator.GetBool("IsSwimming")||p.animator.GetBool("IsWalking")){File.AppendAllText("AgentScripts/warrior-armor-playcheck.txt","Ground calibration requires a fresh idle on land.\n");EditorApplication.update-=verify;return;}float lg=Gap(p,true),rg=Gap(p,false);placement.leftSoleHeight+=.005f-lg;placement.rightSoleHeight+=.005f-rg;stage=1;then=Time.timeAsDouble;return;}
 float left=Gap(p,true),right=Gap(p,false);bool passed=Mathf.Abs(left)<.025f&&Mathf.Abs(right)<.025f;
 File.AppendAllText("AgentScripts/warrior-armor-playcheck.txt",(passed?"PASS ":"FAIL ")+"Fresh idle boot calibration: left gap="+left+"m, right gap="+right+"m\n");
 if(passed){SessionState.SetFloat("WarriorArmor.FinalLeftSole",placement.leftSoleHeight);SessionState.SetFloat("WarriorArmor.FinalRightSole",placement.rightSoleHeight);SessionState.SetBool("WarriorArmor.CalibrationPassed",true);}
 EditorApplication.update-=verify;
 };EditorApplication.update+=verify;result.Log("Calibrating sole clearance after a fresh grounded idle, then independently measuring the result.");
 } static float Gap(ThirdPersonAvatarController p,bool left){var r=p.animator.GetComponentsInChildren<SkinnedMeshRenderer>().Single(x=>x.name=="A1_Boots");var mesh=new UnityEngine.Mesh();r.BakeMesh(mesh,false);var sole=mesh.vertices.Select(v=>r.transform.TransformPoint(v)).Where(v=>(p.transform.InverseTransformPoint(v).x<0)==left).OrderBy(v=>v.y).First();var hit=Physics.RaycastAll(sole+Vector3.up*.6f,Vector3.down,2,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.transform.IsChildOf(p.transform)&&h.normal.y>.4f).OrderBy(h=>h.distance).First();UnityEngine.Object.DestroyImmediate(mesh);return sole.y-hit.point.y;}
}

