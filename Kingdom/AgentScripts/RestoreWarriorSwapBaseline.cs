using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result){File.Copy("SetupBackup/AxeWarrior-2026-11-04/Glowkeeper_Update.unity","Assets/Glowkeeper_Update.unity",true);AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/Glowkeeper_Update.unity",OpenSceneMode.Single);result.Log("Restored model-swap baseline before retrying attachment transfer.");}
}
