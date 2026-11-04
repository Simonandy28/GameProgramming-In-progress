using System;
using Tenkoku.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace GlowKeeper
{
    [DefaultExecutionOrder(1100), DisallowMultipleComponent]
    [AddComponentMenu("GlowKeeper/Tenkoku Forest Schedule")]
    public sealed class TenkokuForestSchedule : MonoBehaviour
    {
        [Header("Tenkoku references")]
        public TenkokuModule sky;
        public TenkokuLib library;
        public Volume moonlightGrading;
        public Renderer[] daylightRays = Array.Empty<Renderer>();
        public Renderer eclipseRenderer;
        [Header("Real gameplay minutes per phase")]
        [Min(0.01f)] public float sunsetMinutes = 3;
        [Min(0.01f)] public float nightMinutes = 30;
        [Min(0.01f)] public float sunriseMinutes = 2;
        [Min(0.01f)] public float daylightMinutes = 2;
        [Header("Sky clock and moonlit visibility")]
        [Range(0,24)] public float sunsetStartHour = 18.5f;
        public int skyYear = 2019, skyMonth = 3, skyDay = 21;
        [Min(0)] public float minimumMoonIntensity = 0.4f;
        public Color nightSkyFill = new Color(0.16f, 0.21f, 0.33f);
        public Color nightHorizonFill = new Color(0.1f, 0.14f, 0.23f);
        public Color nightGroundFill = new Color(0.05f, 0.07f, 0.12f);
        [Header("Sunset color and rays")]
        [Range(0,2)] public float sunsetOrangeStrength=1;
        [Range(0,3)] public float sunRaysStrength=1.2f;
        [Header("Moon over the starting forest vista")]
        [Tooltip("Art-directed placement: keep Tenkoku's textured moon on a visible arc above the starting view. Turn off to use its astronomical position.")]
        public bool frameMoonOverForest=true;
        [Range(5,40)] public float moonMinimumElevation=16;
        [Range(5,40)] public float moonMaximumElevation=24;
        public double ElapsedSeconds { get; private set; }
        public double LoopSeconds => Seconds(sunsetMinutes) + Seconds(nightMinutes) + Seconds(sunriseMinutes) + Seconds(daylightMinutes);
        public string Phase => GetPhase(ElapsedSeconds, sunsetMinutes, nightMinutes, sunriseMinutes, daylightMinutes);
        private DateTime epoch;
        private MaterialPropertyBlock[] rayBlocks;
        private Color[] rayColors;
        private float fogDensity;
        private Vector3 naturalMoonLocalPosition;
        private float forestHeading;
        private MaterialPropertyBlock moonProperties;

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
            double a = Seconds(sunset), b = Seconds(night), c = Seconds(sunrise), d = Seconds(day);
            double t = ((elapsed % (a+b+c+d)) + a+b+c+d) % (a+b+c+d);
            return t < a ? "Sunset" : t < a+b ? "Long moonlit night" : t < a+b+c ? "Sunrise" : "Daylight";
        }

        // Advances actual Tenkoku astronomical time, with a slow night and faster daytime.
        public static double EvaluateSkyHours(double elapsed, float startHour, float sunset, float night, float sunrise, float day)
        {
            double a = Seconds(sunset), b = Seconds(night), c = Seconds(sunrise), d = Seconds(day);
            double duration = a+b+c+d;
            double loops = Math.Floor(elapsed/duration);
            double t = elapsed-loops*duration;
            double hours = t < a ? 2*t/a : t < a+b ? 2+10*(t-a)/b : t < a+b+c ? 12+2*(t-a-b)/c : 14+10*(t-a-b-c)/d;
            return startHour + loops*24 + hours;
        }

        private void Awake()
        {
            if (!sky || !library) { Debug.LogError("Assign Tenkoku Module and Library.", this); enabled=false; return; }
            epoch = new DateTime(skyYear, skyMonth, skyDay);
            fogDensity = RenderSettings.fogDensity;
            naturalMoonLocalPosition=library.moonObject.localPosition;
            forestHeading=Camera.main ? Camera.main.transform.eulerAngles.y : 0;
            moonProperties=new MaterialPropertyBlock();
            library.renderObjectMoon.GetPropertyBlock(moonProperties);
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
            // Restore the native moon before Tenkoku calculates this frame's astronomical data.
            if(library) library.moonObject.localPosition=naturalMoonLocalPosition;
            ElapsedSeconds += Time.deltaTime;
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
            var date = epoch.AddHours(EvaluateSkyHours(ElapsedSeconds, sunsetStartHour, sunsetMinutes, nightMinutes, sunriseMinutes, daylightMinutes));
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
            sun.intensity = Mathf.Min(sun.intensity, 1.1f) * day;
            sun.enabled = day > 0.015f;
            sun.color=Color.Lerp(sun.color,new Color(1,.32f,.065f),sunset*.75f);
            moon.intensity = Mathf.Max(Mathf.Min(moon.intensity,0.55f), minimumMoonIntensity)*night;
            moon.color = new Color(0.58f,0.73f,1);
            moon.enabled = night > 0.015f;
            if(frameMoonOverForest && night>.01f && sky.useCamera)
            {
                float progress=Mathf.Clamp01((float)((ElapsedSeconds%LoopSeconds-Seconds(sunsetMinutes))/Seconds(nightMinutes)));
                float yaw=forestHeading+Mathf.Lerp(-18,18,progress);
                float altitude=Mathf.Lerp(moonMinimumElevation,moonMaximumElevation,Mathf.Sin(progress*Mathf.PI));
                Vector3 direction=Quaternion.Euler(-altitude,yaw,0)*Vector3.forward;
                float distance=Vector3.Distance(library.moonObject.position,sky.useCamera.position);
                library.moonObject.position=sky.useCamera.position+direction*distance;
                library.nightSkyLightObject.rotation=Quaternion.LookRotation(-direction,Vector3.up);
                Shader.SetGlobalVector("Tenkoku_Vec_MoonFwd",direction);
            }
            if(moonProperties!=null)
            {
                moonProperties.SetFloat("_VisibilityBoost",night);
                moonProperties.SetFloat("_VisibleMoonMode",frameMoonOverForest ? 1 : 0);
                library.renderObjectMoon.SetPropertyBlock(moonProperties);
            }
            sun.shadows=LightShadows.Soft;
            moon.shadows=LightShadows.Soft;
            sun.cullingMask=-1; moon.cullingMask=-1;
            RenderSettings.sun = moon.enabled && night>0.5f ? moon : sun;
            library.lightObjectFill.intensity=0.08f*night;
            library.lightObjectFill.color=new Color(0.35f,0.48f,0.7f);
            library.lightObjectFill.enabled=night>0.05f;
            library.lightObjectFill.shadows=LightShadows.None;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=Color.Lerp(nightSkyFill,Color.Lerp(new Color(0.4f,0.45f,0.55f),new Color(.52f,.3f,.15f),sunset*.55f),day);
            RenderSettings.ambientEquatorColor=Color.Lerp(nightHorizonFill,Color.Lerp(new Color(0.29f,0.32f,0.36f),new Color(.4f,.22f,.1f),sunset*.55f),day);
            RenderSettings.ambientGroundColor=Color.Lerp(nightGroundFill,new Color(0.15f,0.17f,0.15f),day);
            RenderSettings.fog=true;
            RenderSettings.fogDensity=fogDensity;
            RenderSettings.fogColor=Color.Lerp(new Color(0.045f,0.07f,0.12f),Color.Lerp(new Color(0.48f,0.43f,0.38f),new Color(.7f,.28f,.07f),sunset*.7f),day);
            RenderSettings.reflectionIntensity=Mathf.Lerp(0.18f,0.8f,day);
            if (moonlightGrading) moonlightGrading.weight=night;
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
