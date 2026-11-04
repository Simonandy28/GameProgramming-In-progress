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
 if(EditorApplication.isPlaying)throw new Exception("Stop Play before fixing locomotion");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var controller=(AnimatorController)p.animator.runtimeAnimatorController;
 foreach(var child in controller.layers[0].stateMachine.states)if(child.state.name=="Walking"||child.state.name=="Running"){child.state.iKOnFeet=false;EditorUtility.SetDirty(child.state);}
 EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(p.gameObject.scene);result.Log("Disabled locomotion foot IK in Walking and Running. Original motion clips, transitions, input, camera and idle grounding are retained.");
 }
}
