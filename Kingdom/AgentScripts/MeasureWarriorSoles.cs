using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var boot=a.GetComponentsInChildren<SkinnedMeshRenderer>().Single(r=>r.name=="A1_Boots");var mesh=new UnityEngine.Mesh();boot.BakeMesh(mesh,false);var points=mesh.vertices.Select(v=>boot.transform.TransformPoint(v)).ToArray();
 foreach(bool left in new[]{true,false}){var side=points.Where(v=>(p.transform.InverseTransformPoint(v).x<0)==left).ToArray();var ankle=a.GetBoneTransform(left?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot);float offset=ankle.position.y-side.Min(v=>v.y);SessionState.SetFloat(left?"WarriorArmor.LeftSole":"WarriorArmor.RightSole",offset);result.Log((left?"Left":"Right")+" ankle to boot sole="+offset+", Unity foot bottom="+(left?a.leftFeetBottomHeight:a.rightFeetBottomHeight));}UnityEngine.Object.DestroyImmediate(mesh);
 }
}
