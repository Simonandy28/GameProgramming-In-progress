using UnityEditor;
using UnityEngine;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 result.Log("Playing="+EditorApplication.isPlaying+" paused="+EditorApplication.isPaused+" scale="+Time.timeScale+" time="+Time.time+" sky="+UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>().ElapsedSeconds);
 }
}
