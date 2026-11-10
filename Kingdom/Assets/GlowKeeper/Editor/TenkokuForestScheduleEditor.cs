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
            EditorGUILayout.HelpBox("Start Offset Minutes selects where the clock begins in the cycle. Tenkoku drives the sun, moon and stars, and the date advances every full cycle.",MessageType.Info);
            DrawDefaultInspector();
            EditorGUILayout.LabelField("Loop length",$"{schedule.LoopSeconds/60:g} gameplay minutes");
            using(new EditorGUI.DisabledScope(!schedule.isActiveAndEnabled))
            {
                if(Application.isPlaying) EditorGUILayout.LabelField("Current phase",schedule.Phase);
                EditorGUILayout.LabelField("Tenkoku previews");
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button("Sunrise")) schedule.SetElapsedTime(0);
                    if(GUILayout.Button("Morning")) schedule.SetElapsedTime((schedule.sunriseMinutes+schedule.daylightMinutes*0.2)*60);
                    if(GUILayout.Button("Noon")) schedule.SetElapsedTime((schedule.sunriseMinutes+schedule.daylightMinutes*0.5)*60);
                }
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button("Afternoon")) schedule.SetElapsedTime((schedule.sunriseMinutes+schedule.daylightMinutes*0.8)*60);
                    if(GUILayout.Button("Sunset")) schedule.SetElapsedTime((schedule.sunriseMinutes+schedule.daylightMinutes+schedule.sunsetMinutes*0.5)*60);
                    if(GUILayout.Button("Night")) schedule.SetElapsedTime((schedule.sunriseMinutes+schedule.daylightMinutes+schedule.sunsetMinutes+schedule.nightMinutes*0.5)*60);
                }
            }
            if(Application.isPlaying) Repaint();
        }
    }
}
