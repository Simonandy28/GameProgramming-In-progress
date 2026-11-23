Shader "GlowKeeper/Tenkoku Celestial Particles URP"
{
    Properties
    {
        _TintColor ("Tint", Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Tenkoku particle texture", 2D) = "white" {}
        _InvFade ("Fade", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background+1603" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off Blend One One
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST, _TintColor;
            float _Tenkoku_Ambient;
            struct Attributes { float4 position:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct Varyings { float4 position:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.position=TransformObjectToHClip(v.position.xyz);
                #if UNITY_REVERSED_Z
                    o.position.z=o.position.w*0.000008;
                #else
                    o.position.z=o.position.w*0.999992;
                #endif
                o.color=v.color;
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color*_TintColor*3;
                c.rgb*=c.a*(1-saturate(_Tenkoku_Ambient*4));
                return half4(c.rgb,0);
            }
            ENDHLSL
        }
    }
}
