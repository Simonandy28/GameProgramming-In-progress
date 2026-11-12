using System;
using Tenkoku.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace GlowKeeper
{
    [ExecuteAlways, DefaultExecutionOrder(1100), DisallowMultipleComponent]
    [AddComponentMenu("GlowKeeper/Tenkoku Forest Schedule")]
    public sealed class TenkokuForestSchedule : MonoBehaviour
    {
        [Header("Tenkoku references")]
        public TenkokuModule sky;
        public TenkokuLib library;
        public Renderer[] daylightRays = Array.Empty<Renderer>();
        public Renderer eclipseRenderer;
        [Header("Real gameplay minutes per phase")]
        [Min(0.01f)] public float sunriseMinutes = 2;
        [Tooltip("Morning, noon and afternoon together.")]
        [Min(0.01f)] public float daylightMinutes = 12;
        [Min(0.01f)] public float sunsetMinutes = 3;
        [Min(0.01f)] public float nightMinutes = 30;
        [Header("Sky clock and moonlit visibility")]
        [FormerlySerializedAs("sunsetStartHour")]
        [Range(0,24)] public float sunriseStartHour = 6f;
        public int skyYear = 2019, skyMonth = 3, skyDay = 21;
        [Header("Editor preview")]
        [Tooltip("Time of day shown in the editor while not playing. Play mode still runs the full schedule.")]
        [Range(0,24)] public float editorPreviewHour = 12f;
        [Min(0)] public float minimumMoonIntensity = 0.4f;
        public Color nightSkyFill = new Color(0.24f, 0.29f, 0.39f);
        public Color nightHorizonFill = new Color(0.23f, 0.25f, 0.30f);
        public Color nightGroundFill = new Color(0.23f, 0.20f, 0.17f);
        [Header("Sunset color and rays")]
        [Range(0,2)] public float sunsetOrangeStrength=1;
        [Range(0,3)] public float sunRaysStrength=1.2f;
        [Header("Forest surface visibility")]
        [Range(0,1)] public float sunlightLimit=0.8f;
        [Range(0,1)] public float shadowStrength=0.65f;
        public double ElapsedSeconds { get; private set; }
        public double LoopSeconds => Seconds(sunsetMinutes) + Seconds(nightMinutes) + Seconds(sunriseMinutes) + Seconds(daylightMinutes);
        public string Phase => GetPhase(ElapsedSeconds, sunsetMinutes, nightMinutes, sunriseMinutes, daylightMinutes);
        private DateTime epoch;
        private MaterialPropertyBlock[] rayBlocks;
        private Color[] rayColors;
        private float fogDensity;

        public float SunsetGlow
        {
            get
            {
                if(!library) return 0;
                float elevation=Mathf.Asin(Mathf.Clamp(-library.sunlightObject.forward.y,-1,1))*Mathf.Rad2Deg;
                return Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,4,elevation))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-16,-3,elevation));
            }
        }

        private static double Seconds(float minutes) => Math.Max(0.01, minutes) * 60;

        public static string GetPhase(double elapsed, float sunset, float night, float sunrise, float day)
        {
            double a = Seconds(sunrise), b = Seconds(day), c = Seconds(sunset), d = Seconds(night);
            double t = ((elapsed % (a+b+c+d)) + a+b+c+d) % (a+b+c+d);
            if (t < a) return "Sunrise";
            if (t < a+b)
            {
                double daylightProgress = (t-a)/b;
                return daylightProgress < 0.4 ? "Morning" : daylightProgress < 0.6 ? "Noon" : "Afternoon";
            }
            return t < a+b+c ? "Sunset" : "Night";
        }

        // Advances actual Tenkoku astronomical time, with a slow night and faster daytime.
        public static double EvaluateSkyHours(double elapsed, float startHour, float sunset, float night, float sunrise, float day)
        {
            double a = Seconds(sunrise), b = Seconds(day), c = Seconds(sunset), d = Seconds(night);
            double duration = a+b+c+d;
            double loops = Math.Floor(elapsed/duration);
            double t = elapsed-loops*duration;
            // At the default start: 06:00 -> 08:00 -> 18:00 -> 20:00 -> 06:00 next day.
            double hours = t < a ? 2*t/a : t < a+b ? 2+10*(t-a)/b : t < a+b+c ? 12+2*(t-a-b)/c : 14+10*(t-a-b-c)/d;
            return startHour + loops*24 + hours;
        }

        private void OnEnable()
        {
            if (!sky || !library) { Debug.LogError("Assign Tenkoku Module and Library.", this); enabled=false; return; }
            epoch = new DateTime(skyYear, skyMonth, skyDay);
            fogDensity = RenderSettings.fogDensity;
            rayBlocks = new MaterialPropertyBlock[daylightRays.Length];
            rayColors = new Color[daylightRays.Length];
            for (int i=0; i<daylightRays.Length; i++)
            {
                rayBlocks[i] = new MaterialPropertyBlock();
                if (!daylightRays[i]) continue;
                daylightRays[i].GetPropertyBlock(rayBlocks[i]);
                var material = daylightRays[i].sharedMaterial;
                rayColors[i] = material && material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white;
            }
            SetElapsedTime(0);
        }

        private void Update()
        {
            // Only supplies the clock. Tenkoku owns all celestial transforms and phases.
            if(Application.isPlaying) ElapsedSeconds += Time.deltaTime;
            ApplySkyClock();
        }

        public void SetElapsedTime(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds<0) throw new ArgumentOutOfRangeException(nameof(seconds));
            ElapsedSeconds=seconds;
            if (sky) ApplySkyClock();
        }

        private void ApplySkyClock()
        {
            epoch = new DateTime(skyYear, skyMonth, skyDay);
            double hours = Application.isPlaying
                ? EvaluateSkyHours(ElapsedSeconds, sunriseStartHour, sunsetMinutes, nightMinutes, sunriseMinutes, daylightMinutes)
                : editorPreviewHour;
            var date = epoch.AddHours(hours);
            sky.autoTime = false;
            sky.useAutoTime = false;
            sky.autoTimeSync = false;
            sky.autoDateSync = false;
            sky.currentYear=date.Year; sky.currentMonth=date.Month; sky.currentDay=date.Day;
            sky.currentHour=date.Hour; sky.currentMinute=date.Minute; sky.currentSecond=date.Second;
        }

        private void LateUpdate() { if (sky && library) ApplyForestLighting(); }

        public void ApplyForestLighting()
        {
            float elevation = Mathf.Asin(Mathf.Clamp(-library.sunlightObject.forward.y,-1,1))*Mathf.Rad2Deg;
            float day = Mathf.SmoothStep(0,1,Mathf.InverseLerp(-10,10,elevation));
            float night=1-day;
            float sunset=SunsetGlow;
            Shader.SetGlobalFloat("_GlowKeeperSunsetAmount",sunset*sunsetOrangeStrength);
            sky.useAmbient = false; // URP fill is applied directly, independent of legacy sky-camera effects.
            var sun = library.lightObjectWorld;
            var moon = library.lightObjectNight;
            sun.intensity = Mathf.Min(sun.intensity, sunlightLimit) * day;
            sun.enabled = day > 0.015f;
            sun.color=Color.Lerp(sun.color,new Color(1,.32f,.065f),sunset*.75f);
            moon.intensity = Mathf.Max(Mathf.Min(moon.intensity,0.55f), minimumMoonIntensity)*night;
            moon.color = new Color(0.58f,0.73f,1);
            moon.enabled = night > 0.015f;
            sun.shadows=LightShadows.Soft;
            moon.shadows=LightShadows.Soft;
            sun.shadowStrength=shadowStrength;
            moon.shadowStrength=shadowStrength;
            sun.cullingMask=-1; moon.cullingMask=-1;
            RenderSettings.sun = moon.enabled && night>0.5f ? moon : sun;
            library.lightObjectFill.intensity=0.08f*night;
            library.lightObjectFill.color=new Color(0.35f,0.48f,0.7f);
            library.lightObjectFill.enabled=night>0.05f;
            library.lightObjectFill.shadows=LightShadows.None;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=Color.Lerp(nightSkyFill,Color.Lerp(new Color(0.48f,0.52f,0.60f),new Color(.54f,.43f,.34f),sunset*.55f),day);
            RenderSettings.ambientEquatorColor=Color.Lerp(nightHorizonFill,Color.Lerp(new Color(0.39f,0.41f,0.43f),new Color(.46f,.35f,.27f),sunset*.55f),day);
            RenderSettings.ambientGroundColor=Color.Lerp(nightGroundFill,new Color(0.32f,0.29f,0.24f),day);
            RenderSettings.fog=true;
            RenderSettings.fogDensity=fogDensity;
            RenderSettings.fogColor=Color.Lerp(new Color(0.045f,0.07f,0.12f),Color.Lerp(new Color(0.48f,0.43f,0.38f),new Color(.7f,.28f,.07f),sunset*.7f),day);
            RenderSettings.reflectionIntensity=Mathf.Lerp(0.18f,0.8f,day);
            if (eclipseRenderer) eclipseRenderer.enabled=false;
            if (rayBlocks == null) return;
            for (int i=0;i<daylightRays.Length;i++)
            {
                if (!daylightRays[i]) continue;
                float rayDay=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-1,8,elevation));
                var color=rayColors[i]; color.a*=rayDay;
                rayBlocks[i].SetColor("_BaseColor",color);
                daylightRays[i].SetPropertyBlock(rayBlocks[i]);
                daylightRays[i].enabled=rayDay>0.015f;
            }
        }
    }
}
