using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 foreach(string path in new[]{"Assets/Wizard/source/Jumanji_AllAnim.fbx","Assets/WizardPBR/Animations/WalkForward.fbx","Assets/WizardPBR/Animations/BattleRunForward.fbx"})foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")))result.Log(path+": "+clip.name+" length="+clip.length+" humanoid="+clip.humanMotion+" loop="+clip.isLooping);
 }
}
