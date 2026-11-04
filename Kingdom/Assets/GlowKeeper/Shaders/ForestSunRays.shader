Shader "Hidden/GlowKeeper/Forest Sun Rays"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _ForestRaySun;
        half4 _ForestRayColor;
        TEXTURE2D_X(_ForestSunRaysTexture);
        TEXTURE2D_X_FLOAT(_ForestSunRayDepth);

        half4 Mask(Varyings input):SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float depth = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord).r;
            #if UNITY_REVERSED_Z
                float sky = 1.0 - step(0.0001, depth);
            #else
                float sky = step(0.9999, depth);
            #endif
            float distanceToSun = length((input.texcoord - _ForestRaySun.xy) * float2(_ForestRaySun.z, 1.0));
            // Sharper falloff near sun source
            float illumination = pow(saturate(1.0 - distanceToSun / 0.90), 1.8);
            return sky * illumination;
        }

        half4 Scatter(Varyings input):SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = input.texcoord;
            // 52 radial steps with sharper falloff for crisp light shaft delineation
            float2 stepToSun = (_ForestRaySun.xy - uv) * (0.95 / 52.0);
            float sum = 0.0, decay = 1.0, weight = 0.0;
            UNITY_UNROLL for(int i = 0; i < 52; i++)
            {
                uv += stepToSun;
                float sampleVal = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).r;
                sum += sampleVal * decay;
                weight += decay;
                decay *= 0.978; // sharper decay keeps tree branch edges distinct
            }
            return sum / max(weight, 0.001);
        }

        half4 Composite(Varyings input):SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
            float rawRays = SAMPLE_TEXTURE2D_X(_ForestSunRaysTexture, sampler_LinearClamp, input.texcoord).r;
            float depth = SAMPLE_TEXTURE2D_X(_ForestSunRayDepth, sampler_PointClamp, input.texcoord).r;
            #if UNITY_REVERSED_Z
                float sky = 1.0 - step(0.0001, depth);
            #else
                float sky = step(0.9999, depth);
            #endif

            // Enhance contrast of light shafts: bright shafts pop, dark gaps between branches remain dark
            float definedRays = pow(saturate(rawRays * 1.30), 1.45) * 1.65;

            scene.rgb += definedRays * _ForestRayColor.rgb * _ForestRaySun.w * 0.55 * lerp(1.0, 0.2, sky);
            return scene;
        }
        ENDHLSL

        Pass
        {
            Name "Depth Occlusion"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Mask
            ENDHLSL
        }
        Pass
        {
            Name "Light Scattering"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Scatter
            ENDHLSL
        }
        Pass
        {
            Name "Golden Rays"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Composite
            ENDHLSL
        }
    }
}
