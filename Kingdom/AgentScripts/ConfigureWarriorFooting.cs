using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 var player=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();
 const string dir="Assets/GlowKeeper/Animation/AxeWarrior";
 if(!AssetDatabase.IsValidFolder(dir))AssetDatabase.CreateFolder("Assets/GlowKeeper/Animation","AxeWarrior");
 string path=dir+"/AxeWarriorAnimator.controller";
 if(!AssetDatabase.LoadAssetAtPath<AnimatorController>(path))AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(player.animator.runtimeAnimatorController),path);
 var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);controller.name="AxeWarriorAnimator";
 foreach(var layer in controller.layers)foreach(var child in layer.stateMachine.states){var state=child.state;if(new[]{"Idle","Walking","Running","Jump","Swimming","TreadingWater"}.Contains(state.name)){state.iKOnFeet=new[]{"Idle","Walking","Running"}.Contains(state.name);EditorUtility.SetDirty(state);}}
 player.animator.runtimeAnimatorController=controller;player.animator.stabilizeFeet=true;
 EditorUtility.SetDirty(player.animator);EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
 EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveScene(player.gameObject.scene);
 result.Log("Created warrior-specific copy of the existing animation controller: foot IK enabled for idle/walk/run and disabled during jump/swim. State names, clips, transitions, input, movement and camera behavior preserved.");
 }
}
