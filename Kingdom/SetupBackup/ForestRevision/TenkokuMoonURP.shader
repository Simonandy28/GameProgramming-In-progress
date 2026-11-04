Shader "GlowKeeper/Tenkoku Moon URP"
{
    Properties
    {
        _MainTex ("Tenkoku moon texture", 2D) = "white" {}
        _PrimaryTint ("Tint", Color) = (1,1,1,1)
        _Color ("Color", Color) = (1,1,1,1)
        _AmbientTint ("Ambient", Color) = (0,0,0,1)
        _overBright ("Brightness", Float) = 1
        _dispStrength ("Displacement", Float) = 1
        _VisibilityBoost ("Night visibility", Range(0,1)) = 0
        _VisibleMoonMode ("Visible forest moon", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background+1605" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST, _PrimaryTint;
            float _VisibilityBoost, _VisibleMoonMode;
            float4 Tenkoku_Vec_SunFwd, Tenkoku_MoonLightColor;
            struct Attributes { float4 position : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 position : SV_POSITION; float3 normal : TEXCOORD0; float2 uv : TEXCOORD1; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.position = TransformObjectToHClip(v.position.xyz);
                #if UNITY_REVERSED_Z
                    o.position.z = o.position.w * 0.000009;
                #else
                    o.position.z = o.position.w * 0.999991;
                #endif
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                half3 textureColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                float phase = smoothstep(-0.08, 0.15, dot(normalize(i.normal), normalize(Tenkoku_Vec_SunFwd.xyz)));
                phase=max(phase,_VisibleMoonMode*_VisibilityBoost*.7);
                half3 moon = textureColor * lerp(0.025, 1.7, phase) * half3(0.8,0.9,1) * _PrimaryTint.rgb;
                return half4(moon,lerp(1,smoothstep(.01,.4,_VisibilityBoost),_VisibleMoonMode));
            }
            ENDHLSL
        }
    }
}
