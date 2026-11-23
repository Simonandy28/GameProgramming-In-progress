Shader "Suimono2/surface_scale" {

Properties {
	//_MainTex ("Particle Texture", 2D) = "white" {}
	_shallowColor ("Tint Color 1", Color) = (0.5,0.5,0.5,0.5)
	_depthColor ("Tint Color 2", Color) = (0.5,0.5,0.5,1.0)
	_BlendColor ("Blend Color", Color) = (0.0,0.0,0.0,0.0)
	_OverlayColor ("Overlay Color", Color) = (0.0,0.0,0.0,0.0)
	_ReflectionColor ("Reflection Color", Color) = (1.0,1.0,1.0,1.0)
	_SpecularColor ("Specular Color", Color) = (1.0,1.0,1.0,1.0)
	_SSSColor ("SubSurface Color", Color) = (1.0,1.0,1.0,1.0)
	_FoamColor ("Foam Color", Color) = (1.0,1.0,1.0,1.0)
	_CausticsColor ("Caustics Color", Color) = (1.0,1.0,1.0,1.0)
	_UnderwaterColor ("Caustics Color", Color) = (1.0,1.0,1.0,1.0)
	
	_NormalTexS ("Shallow Wave Normal Texture", 2D) = "white" {}
	_NormalTexD ("Deep Wave Normal Texture Large", 2D) = "white" {}
	_NormalTexR ("Rolling Wave Normal Texture Large", 2D) = "white" {}
	_ReflectionTex ("reflections", 2D) = "white" {}
	_CubeTex ("Cubemap reflections", CUBE) = "white" {}
	_FoamTex ("Foam Texture", 2D) = "white" {}
	_NormalStrength ("Normal Strength", Range(0.0,1.0)) = 0.5
	_RefractStrength ("Refraction Strength", Range(0.0,1.0)) = 0.5
	_EdgeFade ("Edge Fade", Range(0.01,500.0)) = 1.0
	_EdgeFoamFade ("Edge Foam Amt", Range(5.0,500.0)) = 1.0
	_foamScale ("Foam Scale", Range(5.0,500.0)) = 1.0
	_ShallowFoamAmt ("Shallow Foam Amt", Range(5.0,500.0)) = 1.0
	_HeightFoamAmt ("Edge Foam Amt", Range(5.0,500.0)) = 1.0
	_HeightFoamHeight ("Height Foam Height", Range(5.0,500.0)) = 1.0
	_HeightFoamSpread ("Height Foam Spread", Range(5.0,500.0)) = 1.0

	_DepthFade ("Depth Fade", Range(0.01,5.0)) = 1.0
	_ShallowFade ("Shallow Fade", Range(0.01,5.0)) = 1.0
	_RefractFade ("Refract Fade", Range(0.01,500.0)) = 1.0
	_CausticsFade ("Caustics Fade", Range(0.01,500.0)) = 1.0

_WaveTex ("Wave Texture", 2D) = "white" {}
_shorelineScale ("Shoreline scale", Float) = 1.0
_shorelineFrequency ("Shoreline Frequency", Float) = 1.0
_shorelineSpeed  ("Shoreline Speed", Float) = 1.0
_shorelineHeight  ("Shoreline Height", Float) = 1.0

	_Tess ("Tessellation", Float) = 4.0
    _minDist ("TessMin", Range(-180.0, 0.0)) = 10.0
    _maxDist ("TessMax", Range(20.0, 500.0)) = 25.0

    _suimono_uvx ("uvx1", Float) = 0.0
    _suimono_uvy ("uvy1", Float) = 0.0
}





//SURFACE URP
SubShader {
	Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
	Pass
	{
		Tags { "LightMode"="UniversalForward" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		Cull Back

		HLSLPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

		struct Attributes
		{
			float4 positionOS : POSITION;
			float3 normalOS : NORMAL;
			float4 tangentOS : TANGENT;
			float2 uv : TEXCOORD0;
		};

		struct Varyings
		{
			float4 positionCS : SV_POSITION;
			float3 positionWS : TEXCOORD0;
			float3 normalWS : TEXCOORD1;
			float3 tangentWS : TEXCOORD2;
			float3 bitangentWS : TEXCOORD3;
			float2 uv : TEXCOORD4;
			float4 screenPos : TEXCOORD5;
		};

		TEXTURE2D(_NormalTexS); SAMPLER(sampler_NormalTexS);
		TEXTURE2D(_NormalTexD); SAMPLER(sampler_NormalTexD);
		TEXTURE2D(_NormalTexR); SAMPLER(sampler_NormalTexR);
		TEXTURE2D(_FoamTex); SAMPLER(sampler_FoamTex);
		TEXTURECUBE(_CubeTex); SAMPLER(sampler_CubeTex);

		float4 _shallowColor, _depthColor, _FoamColor, _SpecularColor, _SSSColor, _ReflectionColor;
		float _NormalStrength, _foamScale, _EdgeFade, _DepthFade;
		float4 _suimono_Dir;
		float _suimono_uvx, _suimono_uvy;
		float _heightScale;

		Varyings vert(Attributes input)
		{
			Varyings output;
			float3 posWS = TransformObjectToWorld(input.positionOS.xyz);

			// Dynamic 3D ocean wave displacement
			float wave1 = sin(posWS.x * 0.12 + _Time.y * 1.3) * cos(posWS.z * 0.12 + _Time.y * 0.95);
			float wave2 = sin(posWS.x * 0.05 - _Time.y * 0.7) * sin(posWS.z * 0.05 + _Time.y * 0.6);
			float wave = (wave1 * 0.65 + wave2 * 0.35) * max(0.1, _heightScale);
			posWS.y += wave;

			output.positionCS = TransformWorldToHClip(posWS);
			output.positionWS = posWS;
			output.normalWS = TransformObjectToWorldNormal(input.normalOS);
			output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
			output.bitangentWS = cross(output.normalWS, output.tangentWS) * (input.tangentOS.w > 0.0 ? 1.0 : -1.0);
			output.uv = input.uv;
			output.screenPos = ComputeScreenPos(output.positionCS);
			return output;
		}

		half4 frag(Varyings input) : SV_Target
		{
			float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

			// Animated Suimono rolling wave normal maps
			float2 flow = _suimono_Dir.xz * 0.035;
			float2 uvOffset = float2(_suimono_uvx, _suimono_uvy);

			float2 uv1 = input.positionWS.xz * 0.075 + flow * _Time.y + uvOffset;
			float2 uv2 = input.positionWS.xz * 0.032 - flow * _Time.y * 0.65 + uvOffset * 0.5;
			float2 uv3 = input.positionWS.xz * 0.012 + float2(flow.y, -flow.x) * _Time.y * 0.45;

			half3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexS, sampler_NormalTexS, uv1));
			half3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexD, sampler_NormalTexD, uv2));
			half3 n3 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexR, sampler_NormalTexR, uv3));

			half3 normalTS = normalize(n1 * 0.5 + n2 * 0.35 + n3 * 0.35);
			normalTS.xy *= max(0.1, _NormalStrength) * 1.6;
			normalTS = normalize(normalTS);

			float3x3 tbn = float3x3(normalize(input.tangentWS), normalize(input.bitangentWS), normalize(input.normalWS));
			float3 normalWS = normalize(mul(normalTS, tbn));

			// Screen Depth & Shoreline Calculation
			float2 screenUV = input.screenPos.xy / input.screenPos.w;
			#if UNITY_REVERSED_Z
			float rawDepth = SampleSceneDepth(screenUV);
			#else
			float rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, SampleSceneDepth(screenUV));
			#endif
			float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
			float waterEyeDepth = input.screenPos.w;
			float depthDiff = max(0.0, sceneEyeDepth - waterEyeDepth);

			// Ocean Color Gradient: crystal shallow turquoise -> deep ocean navy
			float depthFactor = saturate(depthDiff / max(0.5, _DepthFade));
			half3 waterColor = lerp(_shallowColor.rgb, _depthColor.rgb, depthFactor);

			// Dynamic Shoreline & Wave Crest Foam
			float2 foamUV = input.positionWS.xz * (_foamScale * 0.004) + flow * _Time.y * 0.5;
			half foamSample = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV).r;
			float shoreFoam = (1.0 - saturate(depthDiff / max(0.2, _EdgeFade))) * foamSample * 1.6;
			float waveFoam = saturate((input.positionWS.y - 3.25) * 2.2) * foamSample * 0.8;
			float totalFoam = saturate(shoreFoam + waveFoam);

			// Lighting from Main Sun/Moon Light
			Light mainLight = GetMainLight();
			float3 lightDir = normalize(mainLight.direction);
			float ndl = saturate(dot(normalWS, lightDir));

			// Sun & Moon specular wave facets
			float3 halfDir = normalize(lightDir + viewDirWS);
			float ndh = saturate(dot(normalWS, halfDir));
			float spec = pow(ndh, 110.0) * 1.6;

			// Sky and Cubemap reflections (Fresnel)
			float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), 4.0);
			half3 reflColor = SAMPLE_TEXTURECUBE(_CubeTex, sampler_CubeTex, reflect(-viewDirWS, normalWS)).rgb;

			// Subsurface scattering glow through wave crests
			float sss = pow(saturate(dot(viewDirWS, -lightDir)), 4.0) * (1.0 - depthFactor);

			// Composite Shading
			half3 ambientLight = float3(0.18, 0.22, 0.28);
			half3 finalColor = waterColor * (mainLight.color * (ndl * 0.6 + 0.4) + ambientLight);
			finalColor += _SSSColor.rgb * sss * mainLight.color * 0.5;
			finalColor = lerp(finalColor, reflColor * _ReflectionColor.rgb * 1.3, fresnel * 0.7);
			finalColor += _SpecularColor.rgb * spec * mainLight.color;
			finalColor = lerp(finalColor, _FoamColor.rgb, totalFoam);

			// Shore transparency fade
			float alpha = saturate(depthDiff / 0.45) * max(_shallowColor.a, 0.85);
			alpha = max(alpha, totalFoam);

			return half4(finalColor, alpha);
		}
		ENDHLSL
	}
}

//SCALE SURFACE DX9 and DX11
SubShader {
	Tags { "Queue"="Geometry+50" "IgnoreProjector"="True" "RenderType"="SuimonoWater" }
	Cull Back Lighting On ZWrite On
	Offset -1,1
	CGPROGRAM

		#include "UnityCG.cginc"
		#include "SuimonoFunctionLibrary.cginc"

		#pragma target 3.0
		#pragma surface SuimonoSurf SuimonoLight addshadow

	ENDCG
}








fallback "Standard"

}
