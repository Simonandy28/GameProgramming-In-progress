using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorUtility.scriptCompilationFailed)throw new Exception("Script compilation failed");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;
 var placement=a.GetComponent<WarriorFootPlacement>();if(!placement)placement=a.gameObject.AddComponent<WarriorFootPlacement>();
 placement.leftSoleHeight=SessionState.GetFloat("WarriorArmor.LeftSole",.07122f);placement.rightSoleHeight=SessionState.GetFloat("WarriorArmor.RightSole",.06967f);
 var local=a.transform.localPosition;local.y=-.065f;a.transform.localPosition=local;
 var controller=(AnimatorController)a.runtimeAnimatorController;var layers=controller.layers;layers[0].iKPass=true;controller.layers=layers;
 EditorUtility.SetDirty(controller);EditorUtility.SetDirty(placement);EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
 result.Log("Boot soles calibrated (L="+placement.leftSoleHeight+", R="+placement.rightSoleHeight+"); model base corrected to Y="+local.y+". Ground-aware resting-foot IK enabled; movement, jumping and swimming keep their existing animation poses.");
 }
}
