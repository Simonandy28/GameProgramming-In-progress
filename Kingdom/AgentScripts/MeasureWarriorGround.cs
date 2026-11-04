using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var boot=a.GetComponentsInChildren<SkinnedMeshRenderer>().Single(r=>r.name=="A1_Boots");var mesh=new UnityEngine.Mesh();boot.BakeMesh(mesh,false);var lowest=mesh.vertices.Select(v=>boot.transform.TransformPoint(v)).OrderBy(v=>v.y).First();
 foreach(var at in new[]{p.transform.position,lowest})
 {var hit=Physics.RaycastAll(at+Vector3.up*.6f,Vector3.down,2f,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider!=p.GetComponent<CharacterController>()&&h.normal.y>.5f).OrderBy(h=>h.distance).First();result.Log("Ground at "+at+" = "+hit.point+" collider="+hit.collider.name+" sole gap="+(lowest.y-hit.point.y));if(at==lowest){SessionState.SetFloat("WarriorArmor.FootOffset",a.transform.localPosition.y+.015f-(lowest.y-hit.point.y));SessionState.SetFloat("WarriorArmor.OriginalSoleGap",lowest.y-hit.point.y);}}
 UnityEngine.Object.DestroyImmediate(mesh);
 }
}
