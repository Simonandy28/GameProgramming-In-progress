using System;
using System.Collections.Generic;
using GlowKeeper;
using UnityEditor;
using UnityEngine;

public static class VerifyDayNightCycle
{
    public static object Main()
    {
        var cycle = UnityEngine.Object.FindAnyObjectByType<ForestDayNightCycle>();
        if (!cycle || !Application.isPlaying) throw new InvalidOperationException("Run in the forest during Play mode.");
        var checkpoints = new[] { (0d, 0f), (15d, 0.5f), (30d, 1f), (900d, 1f), (1829.999d, 1f), (1830d, 1f), (1845d, 0.5f), (1860d, 0f), (1890d, 1f), (3720d, 0f) };
        var results = new List<object>();
        foreach (var checkpoint in checkpoints)
        {
            cycle.SetCycleTime(checkpoint.Item1);
            if (Math.Abs(cycle.NightBlend - checkpoint.Item2) > 0.0001) throw new Exception("Wrong blend at " + checkpoint.Item1);
            if (cycle.sunAndMoon.intensity < 0.2f) throw new Exception("Night light is too dim.");
            if (Math.Abs(cycle.nightColorGrading.weight - checkpoint.Item2) > 0.0001) throw new Exception("Grading failed to follow cycle.");
            if (Math.Abs(RenderSettings.skybox.GetFloat("_NightBlend") - checkpoint.Item2) > 0.0001) throw new Exception("Sky failed to follow cycle.");
            foreach (var ray in cycle.daytimeRays)
                if (checkpoint.Item2 == 1 && ray.enabled) throw new Exception("Daylight ray still visible during night.");
            results.Add(new { seconds = checkpoint.Item1, phase = cycle.Phase, blend = cycle.NightBlend, intensity = cycle.sunAndMoon.intensity });
        }
        // Runtime lighting must use a private material, never dirty the saved morning sky.
        if (RenderSettings.skybox == cycle.cycleSkybox || cycle.cycleSkybox.GetFloat("_NightBlend") != 0) throw new Exception("Saved sky material was modified.");
        cycle.SetCycleTime(0);
        return new { passed = checkpoints.Length, duration = cycle.CycleDurationSeconds, results };
    }
}
