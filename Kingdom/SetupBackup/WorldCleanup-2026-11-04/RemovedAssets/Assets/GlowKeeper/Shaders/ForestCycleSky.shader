Shader "GlowKeeper/Forest Cycle Sky"
{
    Properties
    {
        [NoScaleOffset] _Tex ("Original forest sky", Cube) = "grey" {}
        _Tint ("Daylight tint", Color) = (0.58, 0.58, 0.58, 1)
        _Exposure ("Daylight exposure", Float) = 0.72
        _Rotation ("Sky rotation", Range(0, 360)) = 0
        _NightBlend ("Night blend", Range(0, 1)) = 0

        [Header(Sun Settings)]
        _SunDirection ("Sun direction", Vector) = (-0.96, 0.12, -0.24, 0)
        _SunSize ("Sun angular size", Range(0.01, 0.15)) = 0.042
        _SunColor ("Sun disc color", Color) = (1.0, 0.88, 0.70, 1)
        _SunGlowColor ("Sunset horizon glow", Color) = (1.0, 0.38, 0.08, 1)
        _SunsetFactor ("Sunset glow amount", Range(0, 1)) = 0

        [Header(Moon Settings)]
        _MoonDirection ("Moon direction", Vector) = (0.85, 0.45, -0.25, 0)
        _MoonTex ("Moon texture", 2D) = "white" {}
        _MoonSize ("Moon angular size", Range(0.01, 0.15)) = 0.055
        _MoonColor ("Moon tint", Color) = (1.0, 1.0, 1.0, 1)
        _MoonHaloColor ("Moon halo tint", Color) = (0.35, 0.5, 0.8, 1)
        _MoonIntensity ("Moon brightness", Range(0, 3)) = 1.4

        [Header(Stars)]
        _StarIntensity ("Star intensity", Range(0, 2)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            samplerCUBE _Tex;
            half4 _Tex_HDR, _Tint;
            float _Exposure, _Rotation, _NightBlend;

            // Sun & Sunset
            float4 _SunDirection;
            float _SunSize, _SunsetFactor;
            half4 _SunColor, _SunGlowColor;

            // Moon
            float4 _MoonDirection;
            sampler2D _MoonTex;
            float4 _MoonTex_ST;
            float _MoonSize, _MoonIntensity, _StarIntensity;
            half4 _MoonColor, _MoonHaloColor;

            struct Attributes { float4 vertex : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 position : SV_POSITION; float3 direction : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = UnityObjectToClipPos(input.vertex);
                output.direction = input.vertex.xyz;
                return output;
            }

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            half4 frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float angle = radians(_Rotation);
                float3 skyDirection = float3(cos(angle) * direction.x - sin(angle) * direction.z, direction.y, sin(angle) * direction.x + cos(angle) * direction.z);
                half3 original = DecodeHDR(texCUBE(_Tex, skyDirection), _Tex_HDR);
                half3 daylight = original * _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;

                // --- SUN & SUNSET CALCULATION ---
                float3 sunFwd = normalize(_SunDirection.xyz);
                float sunDot = dot(direction, sunFwd);
                float sunElevation = _SunDirection.y;
                float sunVisible = smoothstep(-0.06, 0.02, sunElevation);

                // Horizon sunset/sunrise color glow
                float horizonBand = exp(-max(0.0, direction.y) * 3.5);
                float sunAtmosphere = pow(saturate(sunDot), 6.0);
                float sunsetGlow = _SunsetFactor * horizonBand * (0.6 + 0.4 * sunAtmosphere);
                half3 sunsetSkyColor = lerp(_SunGlowColor.rgb, half3(1.05, 0.36, 0.08), sunAtmosphere);

                // Defined, non-overexposed Sun Disc
                float sunAngle = acos(clamp(sunDot, -1.0, 1.0));
                float sunRadiusNorm = sunAngle / max(0.0001, _SunSize); // 0 at center, 1 at edge
                float sunDiscEdge = smoothstep(1.01, 0.96, sunRadiusNorm);

                // Subtle limb darkening and gentle core
                float sunLimb = 1.0 - 0.20 * pow(saturate(sunRadiusNorm), 2.0);
                half3 sunCore = _SunColor.rgb * (0.85 * sunLimb);

                // Very soft controlled coronal glow - no blinding blowout
                float sunCorona = pow(saturate(sunDot), 180.0) * 0.12;
                float sunAtmosphereHalo = pow(saturate(sunDot), 16.0) * 0.06;

                half3 sunRender = (sunCore * sunDiscEdge + _SunColor.rgb * (sunCorona + sunAtmosphereHalo)) * sunVisible * (1.0 - _NightBlend * 0.98);

                half3 daySky = lerp(daylight, daylight * 0.65 + sunsetSkyColor, sunsetGlow);
                daySky += sunRender;

                // --- NIGHT & STARS CALCULATION ---
                float elevation = saturate(direction.y);
                half3 night = lerp(half3(0.08, 0.12, 0.22), half3(0.02, 0.035, 0.08), sqrt(elevation));
                float clouds = saturate(dot(original, float3(0.2126, 0.7152, 0.0722)) - 0.3);
                night += clouds * half3(0.025, 0.035, 0.065);

                float2 starUv = float2(atan2(direction.z, direction.x) / (2 * UNITY_PI) + 0.5, asin(clamp(direction.y, -1, 1)) / UNITY_PI + 0.5) * float2(440, 220);
                float2 cell = floor(starUv);
                float2 offset = float2(hash(cell + 7), hash(cell + 19));
                float star = (1 - smoothstep(0.025, 0.13, length(frac(starUv) - offset))) * step(0.982, hash(cell));
                night += star * smoothstep(0.02, 0.2, direction.y) * (1 - saturate(clouds)) * half3(0.7, 0.82, 1.0) * _StarIntensity;

                // --- PROPER MOON CALCULATION ---
                float3 moonFwd = normalize(_MoonDirection.xyz);
                float moonDot = dot(direction, moonFwd);
                float moonVisibility = smoothstep(-0.04, 0.04, moonFwd.y);

                // Lunar atmospheric halo
                float haloWide = pow(saturate(moonDot), 50.0) * 0.35;
                float haloTight = pow(saturate(moonDot), 250.0) * 0.75;
                night += (haloWide + haloTight) * _MoonHaloColor.rgb * _MoonIntensity * moonVisibility;

                // Textured Moon Disc
                float cosMoon = cos(_MoonSize);
                if (moonDot > cosMoon && moonVisibility > 0.01)
                {
                    float3 moonUp = abs(moonFwd.y) < 0.99 ? float3(0, 1, 0) : float3(1, 0, 0);
                    float3 moonRight = normalize(cross(moonUp, moonFwd));
                    moonUp = cross(moonFwd, moonRight);

                    float sinMoon = sin(_MoonSize);
                    float x = dot(direction, moonRight) / max(0.0001, sinMoon);
                    float y = dot(direction, moonUp) / max(0.0001, sinMoon);
                    float r2 = x * x + y * y;

                    if (r2 <= 1.0)
                    {
                        float z = sqrt(max(0.0, 1.0 - r2));
                        float3 sphereNormal = x * moonRight + y * moonUp + z * moonFwd;
                        // Equirectangular mapping
                        float lon = atan2(sphereNormal.x, sphereNormal.z);
                        float lat = asin(clamp(sphereNormal.y, -1.0, 1.0));
                        float2 moonUV = float2(lon / (2.0 * UNITY_PI) + 0.5, lat / UNITY_PI + 0.5);

                        half3 moonTexCol = tex2D(_MoonTex, moonUV).rgb;
                        float limbDarkening = pow(z, 0.35);
                        float edgeAA = smoothstep(1.0, 0.93, sqrt(r2));
                        half3 lunarSurface = moonTexCol * _MoonColor.rgb * (1.35 * limbDarkening) * _MoonIntensity;
                        night = lerp(night, lunarSurface, edgeAA * moonVisibility);
                    }
                }

                // --- FINAL BLEND ---
                half3 finalColor = lerp(daySky, night, _NightBlend);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}