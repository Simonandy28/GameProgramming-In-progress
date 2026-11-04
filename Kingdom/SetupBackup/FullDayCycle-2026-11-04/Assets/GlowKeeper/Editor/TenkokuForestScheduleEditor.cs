using UnityEditor;
using UnityEngine;

namespace GlowKeeper.Editor
{
    [CustomEditor(typeof(TenkokuForestSchedule))]
    public sealed class TenkokuForestScheduleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var schedule=(TenkokuForestSchedule)target;
            EditorGUILayout.HelpBox("Tenkoku drives the sky, sun, moon and stars. This schedule slows down nighttime and keeps forest lighting visible in URP. Starts at sunset on each Play.",MessageType.Info);
            DrawDefaultInspector();
            EditorGUILayout.LabelField("Loop length",$"{schedule.LoopSeconds/60:g} gameplay minutes");
            using(new EditorGUI.DisabledScope(!schedule.isActiveAndEnabled))
            {
                if(Application.isPlaying) EditorGUILayout.LabelField("Current phase",schedule.Phase);
                EditorGUILayout.LabelField("Tenkoku previews");
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button("Sunset")) schedule.SetElapsedTime(0);
                    if(GUILayout.Button("Midnight")) schedule.SetElapsedTime(schedule.sunsetMinutes*60+schedule.nightMinutes*60*0.35);
                    if(GUILayout.Button("Sunrise")) schedule.SetElapsedTime((schedule.sunsetMinutes+schedule.nightMinutes)*60);
                }
            }
            if(Application.isPlaying) Repaint();
        }
    }
}
