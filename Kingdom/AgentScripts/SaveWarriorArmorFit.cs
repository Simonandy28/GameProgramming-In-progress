using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!SessionState.GetBool("WarriorArmor.CalibrationPassed",false))throw new Exception("Final footing calibration has not passed");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var placement=p.animator.GetComponent<WarriorFootPlacement>();placement.leftSoleHeight=SessionState.GetFloat("WarriorArmor.FinalLeftSole",.03f);placement.rightSoleHeight=SessionState.GetFloat("WarriorArmor.FinalRightSole",.03f);
 var source=p.animator.transform.Find("arm").GetComponent<SkinnedMeshRenderer>().sharedMesh;var mesh=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>("Assets/GlowKeeper/Characters/AxeWarrior/LinenSleeves.asset");var vertices=source.vertices;var normals=source.normals;var directions=new Dictionary<Vector3,Vector3>();
 for(int i=0;i<vertices.Length;i++){Vector3 sum;directions.TryGetValue(vertices[i],out sum);directions[vertices[i]]=sum+normals[i];}
 for(int i=0;i<vertices.Length;i++)vertices[i]+=directions[vertices[i]].normalized*.02f;
 mesh.vertices=vertices;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);EditorUtility.SetDirty(placement);
 EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
 result.Log("Saved final calibrated foot offsets L="+placement.leftSoleHeight+", R="+placement.rightSoleHeight+". Sleeve vertex seams share the same cloth expansion to keep the low-poly mesh closed.");
 }
}
