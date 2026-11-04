using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result){EditorApplication.isPaused=false;Application.runInBackground=true;result.Log("Resumed verification playback.");}
}
