Shader "GlowKeeper/Tenkoku Sun URP"
{
    Properties
    {
        _TintColor ("Tint", Color) = (1,1,1,1)
        _CoronaColor ("Corona", Color) = (1,1,1,1)
        _MainTex ("Original BRDF", 2D) = "white" {}
        _overBright ("Brightness", Float) = 1
        _dispStrength ("Displacement", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background+1604" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off Blend One One
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 _TenkokuSunColor;
            struct Attributes { float4 position : POSITION; };
            struct Varyings { float4 position : SV_POSITION; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.position = TransformObjectToHClip(v.position.xyz);
                #if UNITY_REVERSED_Z
                    o.position.z = o.position.w * 0.00001;
                #else
                    o.position.z = o.position.w * 0.99999;
                #endif
                return o;
            }
            half4 frag(Varyings i) : SV_Target { return half4(max(_TenkokuSunColor.rgb, half3(1,0.36,0.08)) * 1.6, 1); }
            ENDHLSL
        }
    }
}
