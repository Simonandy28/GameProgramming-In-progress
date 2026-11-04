using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace GlowKeeper
{
    /// <summary>
    /// Simple, robust Day/Night Cycle for the forest environment.
    /// Cycle:
    ///   1. Sunset (30s)  - Sun sets below the western horizon, skies turn from golden orange to dusk.
    ///   2. Moonlit Night (30 min / 1800s) - A textured, realistic moon rises in the East,
    ///      traverses an astronomical arc across the southern sky reaching zenith at the 15-minute mark,
    ///      and sets in the West. Directional moonlight and shadows dynamically track the moon.
    ///   3. Sunrise (30s) - Sun rises in the East, morning light returns to the forest.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("GlowKeeper/Forest Day Night Cycle")]
    public sealed class ForestDayNightCycle : MonoBehaviour
    {
        // -------------------------------------------------------------
        // 1. CYCLE TIMING (in seconds of gameplay)
        // -------------------------------------------------------------
        [Header("1. Cycle Timing (seconds)")]
        [FormerlySerializedAs("fadeToNightSeconds")]
        [Min(1f), Tooltip("Duration of sunset at the start of the cycle. Default: 30 seconds.")]
        public float sunsetSeconds = 30f;

        [Min(1f), Tooltip("Duration of the moonlit night. Default: 1800 seconds (30 minutes).")]
        public float nightSeconds = 1800f;

        [Min(1f), Tooltip("Duration of sunrise after the moon sets. Default: 30 seconds.")]
        public float sunriseSeconds = 30f;

        // Legacy compatibility property
        public float fadeToNightSeconds
        {
            get => sunsetSeconds;
            set => sunsetSeconds = value;
        }

        // -------------------------------------------------------------
        // 2. SCENE REFERENCES
        // -------------------------------------------------------------
        [Header("2. Scene References")]
        [Tooltip("Single main directional light reused as sunlight and moonlight.")]
        public Light sunAndMoon;

        [Tooltip("Forest skybox material using GlowKeeper/Forest Cycle Sky shader.")]
        public Material cycleSkybox;

        [Tooltip("Post-processing Volume for night color grading.")]
        public Volume nightColorGrading;

        [Tooltip("Sunray mesh/particle renderers in the forest that fade out at night.")]
        public Renderer[] daytimeRays = Array.Empty<Renderer>();

        // -------------------------------------------------------------
        // 3. AUDIO (Ambient Forest Sounds)
        // -------------------------------------------------------------
        [Header("3. Ambient Audio")]
        [Tooltip("Daytime ambient sound (birds, breeze, forest atmosphere).")]
        public AudioClip dayAmbienceClip;
        [Tooltip("Night ambient sound (crickets, owls, night breeze).")]
        public AudioClip nightAmbienceClip;
        [Tooltip("Wind breeze sound.")]
        public AudioClip windClip;

        [Range(0, 1)] public float dayAudioVolume = 0.65f;
        [Range(0, 1)] public float nightAudioVolume = 0.55f;
        [Range(0, 1)] public float windAudioVolume = 0.35f;

        // -------------------------------------------------------------
        // 4. SUNSET SETTINGS (Phase 1: 0 to 30s)
        // -------------------------------------------------------------
        [Header("4. Sunset Settings (Starts at t=0, lasts 30s)")]
        [ColorUsage(false)] public Color sunsetLightColor = new Color(1.0f, 0.65f, 0.38f);
        [Min(0)] public float sunsetIntensity = 0.75f;
        [Range(0, 360), Tooltip("Azimuth angle of the sunset (270 = West).")]
        public float sunsetAzimuth = 270f;
        [Range(-20, 45), Tooltip("Sun elevation above horizon at cycle start (t=0).")]
        public float sunsetStartAltitude = 8f;
        [Range(-30, 0), Tooltip("Sun elevation below horizon when sunset completes (t=30s).")]
        public float sunsetEndAltitude = -8f;
        public Color sunsetSkyAmbient = new Color(0.48f, 0.35f, 0.28f);
        public Color sunsetHorizonAmbient = new Color(0.38f, 0.25f, 0.18f);
        public Color sunsetGroundAmbient = new Color(0.28f, 0.24f, 0.20f);
        public Color sunsetFog = new Color(0.55f, 0.32f, 0.18f);

        // -------------------------------------------------------------
        // 5. MOONLIT NIGHT SETTINGS (Phase 2: 30s to 1830s = 30 mins)
        // -------------------------------------------------------------
        [Header("5. Moonlit Night Settings (Moon moves across sky for 30 minutes)")]
        [ColorUsage(false)] public Color moonLightColor = new Color(0.60f, 0.76f, 1.0f);
        [Min(0)] public float moonIntensity = 0.50f;
        [Range(0, 360), Tooltip("Where the moon rises on the horizon (East = 90-100).")]
        public float moonRiseAzimuth = 95f;
        [Range(0, 360), Tooltip("Where the moon sets on the horizon (West = 265-275).")]
        public float moonSetAzimuth = 275f;
        [Range(10, 85), Tooltip("Maximum altitude (zenith) of the moon at the 15-minute midpoint.")]
        public float moonMaxAltitude = 55f;

        // Rich forest night ambient colors (preserves warm wood and bark tones)
        [Tooltip("Ambient sky dome fill at night.")]
        public Color nightSkyAmbient = new Color(0.30f, 0.36f, 0.50f);
        [Tooltip("Ambient horizon fill at night (soft blue-grey bounce).")]
        public Color nightHorizonAmbient = new Color(0.28f, 0.30f, 0.38f);
        [Tooltip("Ambient ground bounce at night: Dark warm brown so tree trunks, rocks, and earth remain visible dark-brown rather than pitch black.")]
        public Color nightGroundAmbient = new Color(0.32f, 0.26f, 0.22f);
        public Color nightFog = new Color(0.06f, 0.09f, 0.16f);
        [Range(0, 1)] public float nightReflections = 0.22f;

        // -------------------------------------------------------------
        // 6. SUNRISE & DAYLIGHT SETTINGS (Phase 3: 1830s to 1860s)
        // -------------------------------------------------------------
        [Header("6. Sunrise & Daylight Settings")]
        [ColorUsage(false)] public Color sunriseLightColor = new Color(1.0f, 0.70f, 0.40f);
        [ColorUsage(false)] public Color morningLightColor = new Color(1.0f, 0.957f, 0.839f);
        [Min(0)] public float daylightIntensity = 0.80f;
        [Range(0, 360), Tooltip("Azimuth angle of sunrise (90 = East).")]
        public float sunriseAzimuth = 90f;
        [Range(-30, 0)] public float sunriseStartAltitude = -8f;
        [Range(0, 45)] public float sunriseEndAltitude = 15f;
        public Color morningSkyAmbient = new Color(0.46f, 0.52f, 0.62f);
        public Color morningHorizonAmbient = new Color(0.32f, 0.36f, 0.40f);
        public Color morningGroundAmbient = new Color(0.28f, 0.26f, 0.24f);
        public Color morningFog = new Color(0.627f, 1.0f, 0.976f);
        [Range(0, 1)] public float dayReflections = 0.80f;

        // -------------------------------------------------------------
        // RUNTIME STATE
        // -------------------------------------------------------------
        public double CycleElapsedSeconds { get; private set; }
        public double CycleDurationSeconds => Safe(sunsetSeconds) + Safe(nightSeconds) + Safe(sunriseSeconds);

        public float NightBlend => EvaluateNightBlend(CycleElapsedSeconds);
        public float SunsetFactor => EvaluateSunsetFactor(CycleElapsedSeconds);

        public string CurrentPhase
        {
            get
            {
                double s = Safe(sunsetSeconds);
                double n = Safe(nightSeconds);
                double t = GetNormalizedTime(CycleElapsedSeconds);
                if (t < s) return $"Sunset ({t:F1}s / {s:F0}s)";
                if (t < s + n) return $"Moonlit Night ({TimeSpan.FromSeconds(t - s):mm\\:ss} / {TimeSpan.FromSeconds(n):mm\\:ss})";
                return $"Sunrise ({t - s - n:F1}s / {Safe(sunriseSeconds):F0}s)";
            }
        }

        public Vector3 CurrentSunDirection { get; private set; }
        public Vector3 CurrentMoonDirection { get; private set; }
        public float CurrentMoonAltitude { get; private set; }
        public float CurrentMoonAzimuth { get; private set; }

        private Material runtimeSky;
        private Material previousSky;
        private Light previousSun;
        private AmbientMode previousAmbientMode;
        private Color previousSkyAmbient, previousHorizonAmbient, previousGroundAmbient, previousFog;
        private float previousReflection, previousLightIntensity, previousVolumeWeight;
        private Color previousLightColor;
        private Quaternion previousLightRotation;
        private bool previousTemperature, initialized;

        private AudioSource dayAudioSource;
        private AudioSource nightAudioSource;
        private AudioSource windAudioSource;

        private MaterialPropertyBlock[] rayProperties;
        private Color[] rayColors;
        private int[] rayColorIds;
        private bool[] rayEnabled;

        private static readonly int NightBlendId = Shader.PropertyToID("_NightBlend");
        private static readonly int SunsetFactorId = Shader.PropertyToID("_SunsetFactor");
        private static readonly int SunDirectionId = Shader.PropertyToID("_SunDirection");
        private static readonly int MoonDirectionId = Shader.PropertyToID("_MoonDirection");

        private static double Safe(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 1.0 : Math.Max(0.1, value);

        private double GetNormalizedTime(double elapsed)
        {
            double duration = CycleDurationSeconds;
            if (duration <= 0.001) return 0;
            return ((elapsed % duration) + duration) % duration;
        }

        public float EvaluateNightBlend(double elapsed)
        {
            double s = Safe(sunsetSeconds);
            double n = Safe(nightSeconds);
            double r = Safe(sunriseSeconds);
            double t = GetNormalizedTime(elapsed);

            if (t < s) return Mathf.SmoothStep(0f, 1f, (float)(t / s));
            if (t < s + n) return 1f;
            return Mathf.SmoothStep(1f, 0f, (float)((t - s - n) / r));
        }

        public float EvaluateSunsetFactor(double elapsed)
        {
            double s = Safe(sunsetSeconds);
            double t = GetNormalizedTime(elapsed);
            if (t < s) return Mathf.SmoothStep(1f, 0f, (float)(t / s));
            return 0f;
        }

        public static Vector3 DirectionFromAzAlt(float azimuthDegrees, float altitudeDegrees)
        {
            float altRad = altitudeDegrees * Mathf.Deg2Rad;
            float azRad = azimuthDegrees * Mathf.Deg2Rad;
            float cosAlt = Mathf.Cos(altRad);
            return new Vector3(
                Mathf.Sin(azRad) * cosAlt,
                Mathf.Sin(altRad),
                Mathf.Cos(azRad) * cosAlt
            ).normalized;
        }

        private void OnEnable()
        {
            InitializeCycle();
        }

        private void InitializeCycle()
        {
            if (!sunAndMoon)
            {
                var sunMoonGo = GameObject.Find("Sun and Moon Light");
                if (sunMoonGo) sunAndMoon = sunMoonGo.GetComponent<Light>();
            }

            if (!cycleSkybox)
            {
#if UNITY_EDITOR
                cycleSkybox = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/GlowKeeper/ForestSettings/DayNightSky.mat");
#endif
            }

            if (!nightColorGrading)
            {
                var gradingGo = GameObject.Find("Moonlight Color Grading");
                if (gradingGo) nightColorGrading = gradingGo.GetComponent<Volume>();
            }

            // Audio clips auto-assignment if null
            if (!dayAmbienceClip)
            {
#if UNITY_EDITOR
                dayAmbienceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TENKOKU - DYNAMIC SKY/SOUNDS/tenkoku_dayambient.wav");
#endif
            }
            if (!nightAmbienceClip)
            {
#if UNITY_EDITOR
                nightAmbienceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TENKOKU - DYNAMIC SKY/SOUNDS/tenkoku_nightambient.wav");
#endif
            }
            if (!windClip)
            {
#if UNITY_EDITOR
                windClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TENKOKU - DYNAMIC SKY/SOUNDS/tenkoku_windambient.wav");
#endif
            }

            // Setup audio sources
            SetupAudioSources();

            // Cache previous environment settings
            previousSky = RenderSettings.skybox;
            previousSun = RenderSettings.sun;
            previousAmbientMode = RenderSettings.ambientMode;
            previousSkyAmbient = RenderSettings.ambientSkyColor;
            previousHorizonAmbient = RenderSettings.ambientEquatorColor;
            previousGroundAmbient = RenderSettings.ambientGroundColor;
            previousFog = RenderSettings.fogColor;
            previousReflection = RenderSettings.reflectionIntensity;

            if (sunAndMoon)
            {
                previousLightIntensity = sunAndMoon.intensity;
                previousLightColor = sunAndMoon.color;
                previousLightRotation = sunAndMoon.transform.rotation;
                previousTemperature = sunAndMoon.useColorTemperature;

                sunAndMoon.gameObject.SetActive(true);
                sunAndMoon.enabled = true;
                sunAndMoon.useColorTemperature = false;
                RenderSettings.sun = sunAndMoon;
            }

            if (nightColorGrading)
            {
                previousVolumeWeight = nightColorGrading.weight;
            }

            if (cycleSkybox)
            {
                runtimeSky = new Material(cycleSkybox) { name = "Forest Sky (Runtime/Preview)", hideFlags = HideFlags.DontSave };
                RenderSettings.skybox = runtimeSky;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            CacheDaytimeRays();

            initialized = true;
            SetCycleTime(0);
        }

        private void SetupAudioSources()
        {
            if (dayAudioSource == null)
            {
                dayAudioSource = GetOrCreateAudioSource("Forest Day Ambience");
                dayAudioSource.clip = dayAmbienceClip;
                dayAudioSource.loop = true;
                dayAudioSource.playOnAwake = false;
            }
            if (nightAudioSource == null)
            {
                nightAudioSource = GetOrCreateAudioSource("Forest Night Ambience");
                nightAudioSource.clip = nightAmbienceClip;
                nightAudioSource.loop = true;
                nightAudioSource.playOnAwake = false;
            }
            if (windAudioSource == null)
            {
                windAudioSource = GetOrCreateAudioSource("Forest Wind Ambience");
                windAudioSource.clip = windClip;
                windAudioSource.loop = true;
                windAudioSource.playOnAwake = false;
            }

            if (Application.isPlaying)
            {
                if (dayAudioSource.clip && !dayAudioSource.isPlaying) dayAudioSource.Play();
                if (nightAudioSource.clip && !nightAudioSource.isPlaying) nightAudioSource.Play();
                if (windAudioSource.clip && !windAudioSource.isPlaying) windAudioSource.Play();
            }
        }

        private AudioSource GetOrCreateAudioSource(string childName)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                child = go.transform;
            }
            var src = child.GetComponent<AudioSource>();
            if (src == null) src = child.gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f; // 2D ambient sound covering the entire forest
            return src;
        }

        private void Update()
        {
            if (!initialized) return;

            if (Application.isPlaying)
            {
                CycleElapsedSeconds = (CycleElapsedSeconds + Time.deltaTime) % CycleDurationSeconds;
                ApplyCycleLighting();
                UpdateAudio();
            }
        }

        private void UpdateAudio()
        {
            float night = EvaluateNightBlend(CycleElapsedSeconds);
            float dayFactor = 1f - night;

            if (dayAudioSource != null)
            {
                dayAudioSource.volume = dayFactor * dayAudioVolume;
                if (!dayAudioSource.isPlaying && dayAudioSource.clip && dayAudioSource.volume > 0.001f)
                    dayAudioSource.Play();
            }
            if (nightAudioSource != null)
            {
                nightAudioSource.volume = night * nightAudioVolume;
                if (!nightAudioSource.isPlaying && nightAudioSource.clip && nightAudioSource.volume > 0.001f)
                    nightAudioSource.Play();
            }
            if (windAudioSource != null)
            {
                windAudioSource.volume = windAudioVolume;
                if (!windAudioSource.isPlaying && windAudioSource.clip)
                    windAudioSource.Play();
            }
        }

        public void SetCycleTime(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds)) return;
            CycleElapsedSeconds = GetNormalizedTime(seconds);
            if (initialized)
            {
                ApplyCycleLighting();
                if (Application.isPlaying) UpdateAudio();
            }
        }

        public void ApplyCycleLighting()
        {
            if (!sunAndMoon || !runtimeSky) return;

            double s = Safe(sunsetSeconds);
            double n = Safe(nightSeconds);
            double r = Safe(sunriseSeconds);
            double t = GetNormalizedTime(CycleElapsedSeconds);

            float night = EvaluateNightBlend(CycleElapsedSeconds);
            float sunset = EvaluateSunsetFactor(CycleElapsedSeconds);

            Vector3 sunDir;
            Vector3 moonDir;
            Color lightCol;
            float lightInt;
            Quaternion lightRot;

            Color skyAmb, horAmb, gndAmb, fogCol;
            float refl;

            if (t < s)
            {
                // =========================================================
                // PHASE 1: SUNSET (0 to 30s)
                // =========================================================
                float p = (float)(t / s);
                float sunAlt = Mathf.Lerp(sunsetStartAltitude, sunsetEndAltitude, p);
                sunDir = DirectionFromAzAlt(sunsetAzimuth, sunAlt);

                float moonAlt = Mathf.Lerp(-10f, 0f, p);
                moonDir = DirectionFromAzAlt(moonRiseAzimuth, moonAlt);
                CurrentMoonAltitude = moonAlt;
                CurrentMoonAzimuth = moonRiseAzimuth;

                lightRot = Quaternion.LookRotation(-sunDir, Vector3.up);
                lightCol = Color.Lerp(sunsetLightColor, Color.Lerp(sunsetLightColor, moonLightColor, 0.4f), p);
                lightInt = Mathf.Lerp(sunsetIntensity, moonIntensity * 0.5f, p);

                skyAmb = Color.Lerp(sunsetSkyAmbient, nightSkyAmbient, night);
                horAmb = Color.Lerp(sunsetHorizonAmbient, nightHorizonAmbient, night);
                gndAmb = Color.Lerp(sunsetGroundAmbient, nightGroundAmbient, night);
                fogCol = Color.Lerp(sunsetFog, nightFog, night);
                refl = Mathf.Lerp(dayReflections, nightReflections, night);
            }
            else if (t < s + n)
            {
                // =========================================================
                // PHASE 2: MOONLIT NIGHT (30s to 1830s = 30 minutes)
                // =========================================================
                float p = (float)((t - s) / n);

                float moonAz = Mathf.Lerp(moonRiseAzimuth, moonSetAzimuth, p);
                float moonAlt = Mathf.Sin(p * Mathf.PI) * moonMaxAltitude;

                moonDir = DirectionFromAzAlt(moonAz, moonAlt);
                CurrentMoonAltitude = moonAlt;
                CurrentMoonAzimuth = moonAz;

                sunDir = DirectionFromAzAlt(0f, -60f);

                lightRot = Quaternion.LookRotation(-moonDir, Vector3.up);
                lightCol = moonLightColor;
                lightInt = moonIntensity;

                skyAmb = nightSkyAmbient;
                horAmb = nightHorizonAmbient;
                gndAmb = nightGroundAmbient;
                fogCol = nightFog;
                refl = nightReflections;
            }
            else
            {
                // =========================================================
                // PHASE 3: SUNRISE (1830s to 1860s = 30s)
                // =========================================================
                float p = (float)((t - s - n) / r);

                float moonAlt = Mathf.Lerp(0f, -12f, p);
                moonDir = DirectionFromAzAlt(moonSetAzimuth, moonAlt);
                CurrentMoonAltitude = moonAlt;
                CurrentMoonAzimuth = moonSetAzimuth;

                float sunAlt = Mathf.Lerp(sunriseStartAltitude, sunriseEndAltitude, p);
                sunDir = DirectionFromAzAlt(sunriseAzimuth, sunAlt);

                lightRot = Quaternion.LookRotation(-sunDir, Vector3.up);
                lightCol = Color.Lerp(moonLightColor, Color.Lerp(sunriseLightColor, morningLightColor, p), p);
                lightInt = Mathf.Lerp(moonIntensity, daylightIntensity, p);

                skyAmb = Color.Lerp(nightSkyAmbient, morningSkyAmbient, 1f - night);
                horAmb = Color.Lerp(nightHorizonAmbient, morningHorizonAmbient, 1f - night);
                gndAmb = Color.Lerp(nightGroundAmbient, morningGroundAmbient, 1f - night);
                fogCol = Color.Lerp(nightFog, morningFog, 1f - night);
                refl = Mathf.Lerp(nightReflections, dayReflections, 1f - night);
            }

            CurrentSunDirection = sunDir;
            CurrentMoonDirection = moonDir;

            // Apply light
            sunAndMoon.transform.rotation = lightRot;
            sunAndMoon.color = lightCol;
            sunAndMoon.intensity = lightInt;

            // Apply environment ambient & fog
            RenderSettings.ambientSkyColor = skyAmb;
            RenderSettings.ambientEquatorColor = horAmb;
            RenderSettings.ambientGroundColor = gndAmb;
            RenderSettings.fogColor = fogCol;
            RenderSettings.reflectionIntensity = refl;

            // Apply skybox shader parameters
            runtimeSky.SetFloat(NightBlendId, night);
            runtimeSky.SetFloat(SunsetFactorId, sunset);
            runtimeSky.SetVector(SunDirectionId, sunDir);
            runtimeSky.SetVector(MoonDirectionId, moonDir);

            // Apply post-processing grading volume
            if (nightColorGrading) nightColorGrading.weight = night;

            // Apply daytime rays
            UpdateDaytimeRays(night);
        }

        private void UpdateDaytimeRays(float night)
        {
            if (daytimeRays == null || rayProperties == null) return;
            float daylightFactor = 1f - night;
            for (int i = 0; i < daytimeRays.Length; i++)
            {
                var ray = daytimeRays[i];
                if (!ray) continue;
                if (rayColorIds != null && rayColorIds[i] != 0)
                {
                    var color = rayColors[i];
                    color.a *= daylightFactor;
                    rayProperties[i].SetColor(rayColorIds[i], color);
                    ray.SetPropertyBlock(rayProperties[i]);
                }
                ray.enabled = rayEnabled[i] && daylightFactor > 0.01f;
            }
        }

        private void CacheDaytimeRays()
        {
            int length = daytimeRays.Length;
            rayProperties = new MaterialPropertyBlock[length];
            rayColors = new Color[length];
            rayColorIds = new int[length];
            rayEnabled = new bool[length];

            for (int i = 0; i < length; i++)
            {
                var ray = daytimeRays[i];
                if (!ray) continue;
                rayProperties[i] = new MaterialPropertyBlock();
                ray.GetPropertyBlock(rayProperties[i]);
                rayEnabled[i] = ray.enabled;

                var material = ray.sharedMaterial;
                if (!material) continue;
                string property = material.HasProperty("_BaseColor") ? "_BaseColor" :
                                  material.HasProperty("_TintColor") ? "_TintColor" :
                                  material.HasProperty("_Color") ? "_Color" : null;
                if (property == null) continue;
                rayColorIds[i] = Shader.PropertyToID(property);
                rayColors[i] = material.GetColor(property);
            }
        }

        private void OnDisable()
        {
            if (!initialized) return;

            RenderSettings.skybox = previousSky;
            RenderSettings.sun = previousSun;
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientSkyColor = previousSkyAmbient;
            RenderSettings.ambientEquatorColor = previousHorizonAmbient;
            RenderSettings.ambientGroundColor = previousGroundAmbient;
            RenderSettings.fogColor = previousFog;
            RenderSettings.reflectionIntensity = previousReflection;

            if (sunAndMoon)
            {
                sunAndMoon.intensity = previousLightIntensity;
                sunAndMoon.color = previousLightColor;
                sunAndMoon.transform.rotation = previousLightRotation;
                sunAndMoon.useColorTemperature = previousTemperature;
            }

            if (nightColorGrading) nightColorGrading.weight = previousVolumeWeight;

            if (daytimeRays != null && rayProperties != null)
            {
                for (int i = 0; i < daytimeRays.Length; i++)
                {
                    if (!daytimeRays[i]) continue;
                    daytimeRays[i].enabled = rayEnabled[i];
                }
            }

            if (runtimeSky)
            {
                if (Application.isPlaying) Destroy(runtimeSky);
                else DestroyImmediate(runtimeSky);
            }

            initialized = false;
        }
    }
}
