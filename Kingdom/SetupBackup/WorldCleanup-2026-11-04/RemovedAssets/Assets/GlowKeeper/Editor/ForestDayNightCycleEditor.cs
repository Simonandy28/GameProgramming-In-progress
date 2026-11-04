using System;
using UnityEditor;
using UnityEngine;

namespace GlowKeeper.Editor
{
    [CustomEditor(typeof(ForestDayNightCycle))]
    public sealed class ForestDayNightCycleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cycle = (ForestDayNightCycle)target;

            EditorGUILayout.HelpBox(
                "Celestial Cycle Mechanism:\n" +
                " 1. Starts at Sunset (30 seconds): Sun sinks below western horizon, warm golden light fades to dusk.\n" +
                " 2. Moonlit Night (30 minutes / 1800s): Proper textured moon rises in the East, traverses an astronomical arc overhead reaching 55° peak at 15 minutes, and sets in the West. Moonlight dynamically tracks the moon.\n" +
                " 3. Sunrise (30 seconds): Sun rises in the East, morning light returns to the forest.\n\n" +
                "Use the preview buttons or time slider below to test any phase instantly (works in Edit Mode and Play Mode).",
                MessageType.Info
            );

            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Cycle Status & Astronomical Data", EditorStyles.boldLabel);

            var duration = TimeSpan.FromSeconds(cycle.CycleDurationSeconds);
            var elapsed = TimeSpan.FromSeconds(cycle.CycleElapsedSeconds);

            EditorGUILayout.LabelField("Current Phase", cycle.CurrentPhase);
            EditorGUILayout.LabelField("Cycle Clock", $"{elapsed:mm\\:ss} / {duration:mm\\:ss} ({cycle.CycleElapsedSeconds:F1}s)");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), cycle.NightBlend, $"Night Blend: {cycle.NightBlend:P0}");

            if (cycle.NightBlend > 0.01f)
            {
                EditorGUILayout.LabelField("Moon Position", $"Azimuth: {cycle.CurrentMoonAzimuth:F1}° | Altitude: {cycle.CurrentMoonAltitude:F1}°");
            }
            if (cycle.SunsetFactor > 0.01f)
            {
                EditorGUILayout.LabelField("Sunset Glow", $"{cycle.SunsetFactor:P0}");
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Interactive Timeline Scrubber", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            float newTime = EditorGUILayout.Slider(
                "Preview Time (s)",
                (float)cycle.CycleElapsedSeconds,
                0f,
                (float)cycle.CycleDurationSeconds
            );
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(cycle, "Scrub Cycle Time");
                cycle.SetCycleTime(newTime);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Quick Phase Jump Preview", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🌅 Sunset Start (0s)"))
                    {
                        cycle.SetCycleTime(0);
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("🌆 Mid Sunset (15s)"))
                    {
                        cycle.SetCycleTime(cycle.sunsetSeconds * 0.5f);
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("🌕 Moon Rise (30s)"))
                    {
                        cycle.SetCycleTime(cycle.sunsetSeconds);
                        SceneView.RepaintAll();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🌌 Midnight Apex (15m 30s)"))
                    {
                        cycle.SetCycleTime(cycle.sunsetSeconds + cycle.nightSeconds * 0.5f);
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("🌖 Moon Setting (30m 30s)"))
                    {
                        cycle.SetCycleTime(cycle.sunsetSeconds + cycle.nightSeconds);
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("☀️ Sunrise (31m 00s)"))
                    {
                        cycle.SetCycleTime(cycle.sunsetSeconds + cycle.nightSeconds + cycle.sunriseSeconds * 0.95f);
                        SceneView.RepaintAll();
                    }
                }
            }

            if (Application.isPlaying) Repaint();
        }
    }
}
