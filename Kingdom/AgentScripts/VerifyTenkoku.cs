using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlowKeeper;
using UnityEditor;
using UnityEngine;

public static class VerifyTenkoku
{
    public static async Task<object> Main()
    {
        var c=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
        if(!Application.isPlaying || !c) throw new Exception("Enter Play mode.");
        var old=UnityEngine.Object.FindAnyObjectByType<ForestDayNightCycle>();
        if(old.enabled || old.sunAndMoon.enabled) throw new Exception("Old cycle or light is still enabled.");
        var checkpoints=new[] {(0d,"Sunset",18.5),(90d,"Sunset",19.5),(180d,"Long moonlit night",20.5),(810d,"Long moonlit night",24d),(1979.999d,"Long moonlit night",30.4999944),(1980d,"Sunrise",30.5),(2100d,"Daylight",32.5),(2220d,"Sunset",42.5),(4440d,"Sunset",66.5)};
        foreach(var point in checkpoints)
        {
            var phase=TenkokuForestSchedule.GetPhase(point.Item1,3,30,2,2);
            var hour=TenkokuForestSchedule.EvaluateSkyHours(point.Item1,18.5f,3,30,2,2);
            if(phase!=point.Item2 || Math.Abs(hour-point.Item3)>0.0001) throw new Exception("Timeline mismatch at "+point.Item1);
        }
        float oldScale=Time.timeScale;
        bool oldBackground=Application.runInBackground;
        var results=new List<object>();
        try
        {
            Application.runInBackground=true;
            Time.timeScale=0;
            foreach(var t in new[]{0d,810d,1980d})
            {
                c.SetElapsedTime(t);
                await Task.Delay(650);
                if(c.ElapsedSeconds!=t) throw new Exception("Pause did not stop the timer.");
                if(RenderSettings.skybox.shader.name.IndexOf("Tenkoku",StringComparison.OrdinalIgnoreCase)<0) throw new Exception("Tenkoku does not control the skybox.");
                var primary=RenderSettings.sun;
                if(!primary || !primary.enabled || primary.intensity<=0) throw new Exception("Main light is missing.");
                if(t==810 && c.library.lightObjectNight.intensity<c.minimumMoonIntensity) throw new Exception("Night moonlight is too dim.");
                results.Add(new { seconds=t, c.Phase, hour=c.sky.currentHour, minute=c.sky.currentMinute, mainLight=primary.name, intensity=primary.intensity });
            }
            Time.timeScale=1;
            c.SetElapsedTime(179.8);
            await Task.Delay(1300);
            if(c.Phase!="Long moonlit night") throw new Exception("Automatic night transition failed.");
            c.SetElapsedTime(2219.8);
            await Task.Delay(1300);
            if(c.Phase!="Sunset") throw new Exception("Automatic repeat failed.");
            var names=new[]{"GlowKeeper/Tenkoku Sun URP","GlowKeeper/Tenkoku Moon URP","GlowKeeper/Tenkoku Celestial Particles URP","GlowKeeper/Tenkoku Stable Clouds URP","GlowKeeper/Tenkoku Orange Sunset URP","Hidden/GlowKeeper/Forest Sun Rays","TENKOKU/star_shader","TENKOKU/galaxy_shader"};
            foreach(var name in names)
            {
                var shader=Shader.Find(name);
                if(!shader || ShaderUtil.GetShaderMessages(shader).Any(m=>m.severity.ToString()=="Error")) throw new Exception("Shader error: "+name);
            }
            return new { timelineCheckpoints=checkpoints.Length, playModePreviews=results, pausePassed=true, automaticNightPassed=true, repeatPassed=true, shaders=names.Length, loopMinutes=c.LoopSeconds/60 };
        }
        finally { Time.timeScale=oldScale; Application.runInBackground=oldBackground; c.SetElapsedTime(0); }
    }
}
