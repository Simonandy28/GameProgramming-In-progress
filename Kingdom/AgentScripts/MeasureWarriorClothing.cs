using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var a=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>().animator;
 foreach(var r in a.GetComponentsInChildren<SkinnedMeshRenderer>(true))
 {if(r.sharedMesh.blendShapeCount>0)result.Log("BLENDS "+r.name+": "+string.Join(",",Enumerable.Range(0,r.sharedMesh.blendShapeCount).Select(i=>r.sharedMesh.GetBlendShapeName(i)+"="+r.GetBlendShapeWeight(i))));}
 var boot=a.GetComponentsInChildren<SkinnedMeshRenderer>().Single(r=>r.name=="A1_Boots");var mesh=new UnityEngine.Mesh();boot.BakeMesh(mesh,false);var root=a.transform.parent;
 float min=mesh.vertices.Min(v=>boot.transform.TransformPoint(v).y);float gap=min-root.position.y;float delta=.015f-gap;
 SessionState.SetFloat("WarriorArmor.FootOffset",a.transform.localPosition.y+delta);
 result.Log("Measured boot-sole gap="+gap+"m, desired=0.015m. Saved proposed model local Y="+(a.transform.localPosition.y+delta));
 UnityEngine.Object.DestroyImmediate(mesh);
 }
}
