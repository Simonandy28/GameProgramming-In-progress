Shader "GlowKeeper/Suimono Ocean URP"
{
    Properties
    {
        _NormalTexS ("Suimono calm waves", 2D)="bump"{}
        _NormalTexD ("Suimono turbulent waves", 2D)="bump"{}
        _NormalTexR ("Suimono rolling waves", 2D)="bump"{}
        _FoamTex ("Suimono foam", 2D)="white"{}
        _ShallowColor ("Shallow water",Color)=(.12,.52,.55,1)
        _DeepColor ("Deep water",Color)=(.035,.18,.27,1)
        _FoamColor ("Foam",Color)=(.8,.9,.86,1)
        _NormalStrength ("Ripples",Range(0,1))=.45
        _FoamWidth ("Shoreline foam width",Float)=.65
        _FoamEnabled ("Foam enabled",Float)=1
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-20"}
        Pass
        {
            Name "Suimono Ocean"
            Tags {"LightMode"="UniversalForward"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            TEXTURE2D(_NormalTexS);SAMPLER(sampler_NormalTexS);
            TEXTURE2D(_NormalTexD);SAMPLER(sampler_NormalTexD);
            TEXTURE2D(_NormalTexR);SAMPLER(sampler_NormalTexR);
            TEXTURE2D(_FoamTex);SAMPLER(sampler_FoamTex);
            float4 _OceanFlow, _OceanWaves;
            half4 _ShallowColor,_DeepColor,_FoamColor;
            float _NormalStrength,_FoamWidth,_FoamEnabled;
            struct Attributes {float4 positionOS:POSITION;};
            struct Varyings {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float fog:TEXCOORD1;float eyeDepth:TEXCOORD2;};
            float WaveHeight(float2 world)
            {
                float2 uv=world/max(1,_OceanFlow.w);
                float2 flow=_OceanFlow.xy*_Time.y*_OceanFlow.z*.12;
                // SuimonoFunctionLibrary.cginc: calm, turbulent and rolling wave layers.
                float c0=saturate(lerp(-.035,.5,SAMPLE_TEXTURE2D_LOD(_NormalTexS,sampler_NormalTexS,uv+flow,0).a));
                float c1=saturate(lerp(-.035,.5,SAMPLE_TEXTURE2D_LOD(_NormalTexS,sampler_NormalTexS,uv*.75-flow*.25,0).a));
                float t0=saturate(lerp(-.035,.5,SAMPLE_TEXTURE2D_LOD(_NormalTexD,sampler_NormalTexD,uv+flow,0).a));
                float t1=saturate(lerp(-.035,.5,SAMPLE_TEXTURE2D_LOD(_NormalTexD,sampler_NormalTexD,uv*.5-flow*.25,0).a));
                float r0=saturate(lerp(-.035,.5,SAMPLE_TEXTURE2D_LOD(_NormalTexR,sampler_NormalTexR,uv*_OceanWaves.z+flow*.0625,0).a));
                float r1=saturate(lerp(-.035,.5,SAMPLE_TEXTURE2D_LOD(_NormalTexR,sampler_NormalTexR,uv*_OceanWaves.z+flow*.125,0).a));
                float calm=(c0+c1)*.8;
                float turbulent=(t0*.2+t0*t1*.8)*_OceanWaves.w*.5;
                float rolling=r0*4+r1*3;
                return (calm+turbulent-.4)*_OceanWaves.x+(rolling-1.5)*_OceanWaves.y;
            }
            Varyings Vert(Attributes input)
            {
                Varyings o;float3 p=TransformObjectToWorld(input.positionOS.xyz);
                p.y+=WaveHeight(p.xz);
                o.positionWS=p;o.positionCS=TransformWorldToHClip(p);
                o.fog=ComputeFogFactor(o.positionCS.z);o.eyeDepth=-TransformWorldToView(p).z;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float2 screenUV=GetNormalizedScreenSpaceUV(i.positionCS);
                float rawDepth=SampleSceneDepth(screenUV);
                float depth=max(0,LinearEyeDepth(rawDepth,_ZBufferParams)-i.eyeDepth);
                float2 uv=i.positionWS.xz/7;
                float2 flow=_OceanFlow.xy*_Time.y*_OceanFlow.z*.06;
                half3 n1=UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexS,sampler_NormalTexS,uv+flow));
                half3 n2=UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexD,sampler_NormalTexD,uv*.63-flow*.4));
                half3 n=normalize(half3((n1.x+n2.x)*_NormalStrength,1,(n1.y+n2.y)*_NormalStrength));
                half3 view=SafeNormalize(GetWorldSpaceViewDir(i.positionWS));
                Light light=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half illumination=.35+.65*saturate(dot(n,light.direction))*light.shadowAttenuation;
                half3 ambient=max(SampleSH(n),half3(.08,.10,.13));
                half3 water=lerp(_ShallowColor.rgb,_DeepColor.rgb,1-exp(-depth*.2));
                water*=ambient+light.color*illumination*.55;
                half3 refracted=SampleSceneColor(screenUV+n.xz*.006*saturate(depth));
                water=lerp(refracted,water,saturate(depth*.8+.2));
                half fresnel=.04+.55*pow(1-saturate(dot(n,view)),5);
                half3 reflected=GlossyEnvironmentReflection(reflect(-view,n),.22,1);
                water=lerp(water,max(reflected,ambient*half3(.5,.7,.9)),fresnel);
                half spec=pow(saturate(dot(n,SafeNormalize(view+light.direction))),160);
                water+=light.color*spec*light.shadowAttenuation*.65;
                float noise=SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,i.positionWS.xz*.45+flow*.2).r;
                float foam=saturate(1-depth/max(.01,_FoamWidth))*smoothstep(.25,.68,noise)*_FoamEnabled;
                water=lerp(water,_FoamColor.rgb*(ambient+light.color*.5),foam*.8);
                return half4(MixFog(water,i.fog),saturate(depth*3));
            }
            ENDHLSL
        }
    }
}
