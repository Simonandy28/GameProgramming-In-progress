using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlowKeeper;
using UnityEngine;

public static class VerifyDayNightClock
{
    public static async Task<object> Main()
    {
        var cycle = UnityEngine.Object.FindAnyObjectByType<ForestDayNightCycle>();
        if (!Application.isPlaying || !cycle) throw new InvalidOperationException("Enter Play mode first.");
        float previousScale = Time.timeScale;
        bool previousBackground = Application.runInBackground;
        var results = new List<object>();
        try
        {
            Application.runInBackground = true;
            Time.timeScale = 1;
            foreach (var checkpoint in new[] { (29.8d, "Moonlit night"), (1829.8d, "Sunrise"), (1859.8d, "Morning to night") })
            {
                cycle.SetCycleTime(checkpoint.Item1);
                await Task.Delay(1400);
                if (cycle.Phase != checkpoint.Item2) throw new Exception("Live clock did not advance from " + checkpoint.Item1 + "; reached " + cycle.CycleElapsedSeconds);
                results.Add(new { start = checkpoint.Item1, reached = cycle.CycleElapsedSeconds, phase = cycle.Phase });
            }
            Time.timeScale = 0;
            cycle.SetCycleTime(100);
            await Task.Delay(400);
            if (cycle.CycleElapsedSeconds != 100) throw new Exception("Clock advanced while paused.");
            return new { passed = 4, transitions = results, pause = "Timer stayed at 100 seconds while timeScale was zero" };
        }
        finally
        {
            Time.timeScale = previousScale;
            Application.runInBackground = previousBackground;
            cycle.SetCycleTime(0);
        }
    }
}
