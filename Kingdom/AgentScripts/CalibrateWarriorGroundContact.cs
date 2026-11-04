using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var placement=p.animator.GetComponent<WarriorFootPlacement>();
 float leftGap=Gap(p,true),rightGap=Gap(p,false);placement.leftSoleHeight+=.005f-leftGap;placement.rightSoleHeight+=.005f-rightGap;
 SessionState.SetFloat("WarriorArmor.FinalLeftSole",placement.leftSoleHeight);SessionState.SetFloat("WarriorArmor.FinalRightSole",placement.rightSoleHeight);
 double then=Time.timeAsDouble;EditorApplication.CallbackFunction verify=null;
 verify=()=>{if(!EditorApplication.isPlaying||!p){EditorApplication.update-=verify;return;}if(Time.timeAsDouble-then<1.2) return;float left=Gap(p,true),right=Gap(p,false);File.AppendAllText("AgentScripts/warrior-armor-playcheck.txt",((Mathf.Abs(left)<.025f&&Mathf.Abs(right)<.025f)?"PASS ":"FAIL ")+"Final boot calibration: left gap="+left+"m, right gap="+right+"m\n");EditorApplication.update-=verify;};EditorApplication.update+=verify;
 result.Log("Adjusted IK goal-to-sole offsets to remove the remaining visual boot clearance: L="+placement.leftSoleHeight+", R="+placement.rightSoleHeight);
 }
 static float Gap(ThirdPersonAvatarController p,bool left){var r=p.animator.GetComponentsInChildren<SkinnedMeshRenderer>().Single(x=>x.name=="A1_Boots");var mesh=new UnityEngine.Mesh();r.BakeMesh(mesh,false);var sole=mesh.vertices.Select(v=>r.transform.TransformPoint(v)).Where(v=>(p.transform.InverseTransformPoint(v).x<0)==left).OrderBy(v=>v.y).First();var hit=Physics.RaycastAll(sole+Vector3.up*.6f,Vector3.down,2,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.transform.IsChildOf(p.transform)&&h.normal.y>.4f).OrderBy(h=>h.distance).First();UnityEngine.Object.DestroyImmediate(mesh);return sole.y-hit.point.y;}
}
