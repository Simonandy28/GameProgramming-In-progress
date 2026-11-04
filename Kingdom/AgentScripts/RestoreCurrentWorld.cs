using UnityEditor;
using UnityEditor.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 const string current="Assets/Glowkeeper_Update.unity";
 EditorSceneManager.OpenScene(current,OpenSceneMode.Single);
 EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(current,true),new EditorBuildSettingsScene("Assets/Scenes/GlowKeeper Forest.unity",false),new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity",false)};
 result.Log("Current animated-avatar scene restored and selected as the build entry scene.");
 }
}
