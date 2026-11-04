using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result){Application.runInBackground=PlayerSettings.runInBackground;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;result.Log("Saved armored-warrior scene is ready, with Play stopped.");}
}
