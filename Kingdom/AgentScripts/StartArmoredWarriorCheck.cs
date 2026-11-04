using System;
using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result){if(EditorUtility.scriptCompilationFailed)throw new Exception("Compilation failed");EditorApplication.isPlaying=true;result.Log("Starting armored-warrior visual and leg checks.");}
}
