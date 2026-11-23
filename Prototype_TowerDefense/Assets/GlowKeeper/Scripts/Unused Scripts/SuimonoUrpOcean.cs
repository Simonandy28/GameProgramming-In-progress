using Suimono.Core;
using UnityEngine;

namespace GlowKeeper
{
    // URP surface bridge for the imported Built-in Suimono version. Its existing
    // settings, wave/normal textures and wave-height formula drive this surface.
    // Legacy helper cameras and image effects remain disabled in URP.
    [ExecuteAlways, DefaultExecutionOrder(1200), DisallowMultipleComponent]
    public sealed class SuimonoUrpOcean : MonoBehaviour
    {
        public SuimonoObject settings;
        public SuimonoModuleLib library;
        public Renderer oceanRenderer;
        public Material oceanMaterial;
        [Min(1)] public float waveWorldSize=28;
        [Range(.1f,3)] public float shorelineFoamWidth=.65f;
        [Range(0,1)] public float normalStrength=.45f;
        MaterialPropertyBlock properties;

        void OnEnable() { properties=new MaterialPropertyBlock(); ApplySettings(); }
        void LateUpdate() { ApplySettings(); }
        public void ApplySettings()
        {
            if(!settings||!library||!oceanRenderer||!oceanMaterial)return;
            if(properties==null)properties=new MaterialPropertyBlock();
            float radians=settings.flowDirection*Mathf.Deg2Rad;
            properties.SetVector("_OceanFlow",new Vector4(Mathf.Sin(radians),Mathf.Cos(radians),settings.flowSpeed,waveWorldSize));
            properties.SetVector("_OceanWaves",new Vector4(settings.waveHeight,settings.lgWaveHeight,settings.lgWaveScale,settings.turbulenceFactor));
            properties.SetColor("_ShallowColor",settings.shallowColor);
            properties.SetColor("_DeepColor",settings.depthColor);
            properties.SetColor("_FoamColor",settings.foamColor);
            properties.SetFloat("_FoamWidth",shorelineFoamWidth);
            properties.SetFloat("_NormalStrength",normalStrength);
            properties.SetFloat("_FoamEnabled",settings.enableFoam?1:0);
            properties.SetTexture("_NormalTexS",library.texNormalC);
            properties.SetTexture("_NormalTexD",library.texNormalT);
            properties.SetTexture("_NormalTexR",library.texNormalR);
            properties.SetTexture("_FoamTex",library.texFoam);
            oceanRenderer.SetPropertyBlock(properties);
        }
    }
}
