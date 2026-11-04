using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace GlowKeeper
{
    // Depth-masked radial light scattering for URP's Render Graph renderer.
    public sealed class ForestSunRaysFeature : ScriptableRendererFeature
    {
        public Shader raysShader;
        private Material material;
        private RaysPass pass;
        private ForestDayNightCycle cycle;

        public override void Create()
        {
            CoreUtils.Destroy(material);
            if (raysShader) material = CoreUtils.CreateEngineMaterial(raysShader);
            pass = new RaysPass { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!material || renderingData.cameraData.cameraType != CameraType.Game) return;
            if (!cycle) cycle = Object.FindAnyObjectByType<ForestDayNightCycle>();
            if (!cycle || !cycle.isActiveAndEnabled) return;

            var camera = renderingData.cameraData.camera;
            Vector3 direction = cycle.CurrentSunDirection;
            if (direction.sqrMagnitude < 0.001f) return;

            var sun = camera.WorldToViewportPoint(camera.transform.position + direction * 100f);
            float elevation = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
            float visible = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-3f, 4f, elevation));
            if (sun.z <= 0f || sun.x < -0.2f || sun.x > 1.2f || sun.y < -0.2f || sun.y > 1.2f || visible <= 0f) return;

            float edge = Mathf.Clamp01((1.2f - Mathf.Max(Mathf.Abs(sun.x - 0.5f) * 2f, Mathf.Abs(sun.y - 0.5f) * 2f)) / 0.25f);
            float sunsetGlow = cycle.SunsetFactor;
            float night = cycle.NightBlend;
            float strength = 1.85f * visible * edge * (1f - night) * Mathf.Lerp(0.65f, 1.35f, sunsetGlow);
            if (strength <= 0.001f) return;

            Color rayColor = Color.Lerp(new Color(1f, 0.92f, 0.72f), new Color(1f, 0.55f, 0.20f), sunsetGlow);
            pass.Setup(material, new Vector4(sun.x, sun.y, camera.aspect, strength), rayColor);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing) { CoreUtils.Destroy(material); }

        private sealed class RaysPass : ScriptableRenderPass
        {
            private Material material;
            private Vector4 sun;
            private Color color;
            private static readonly int RaysId = Shader.PropertyToID("_ForestSunRaysTexture");
            private static readonly int DepthId = Shader.PropertyToID("_ForestSunRayDepth");
            private sealed class Data
            {
                public TextureHandle source;
                public Material material;
                public int index;
                public Vector4 sun;
                public Color color;
            }

            public RaysPass()
            {
                requiresIntermediateTexture = true;
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }
            public void Setup(Material m, Vector4 s, Color c) { material = m; sun = s; color = c; }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer || !resources.cameraDepthTexture.IsValid()) return;
                var source = resources.activeColorTexture;
                var full = graph.GetTextureDesc(source);
                full.name = "Forest Sun Rays Composite";
                full.clearBuffer = false;
                full.depthBufferBits = DepthBits.None;
                full.msaaSamples = MSAASamples.None;
                var half = full;
                half.width = Mathf.Max(1, half.width / 2);
                half.height = Mathf.Max(1, half.height / 2);
                half.colorFormat = GraphicsFormat.R16_SFloat;
                half.name = "Forest Sun Rays Sky Mask";
                var mask = graph.CreateTexture(half);
                half.name = "Forest Sun Rays Scattering";
                var rays = graph.CreateTexture(half);
                var output = graph.CreateTexture(full);
                Record(graph, "Sun Rays: Tree Occlusion", resources.cameraDepthTexture, mask, 0);
                using (var builder = graph.AddRasterRenderPass<Data>("Sun Rays: Radial Scattering", out var data))
                {
                    Fill(data, mask, 1);
                    builder.UseTexture(mask);
                    builder.SetRenderAttachment(rays, 0, AccessFlags.Write);
                    builder.SetGlobalTextureAfterPass(rays, RaysId);
                    builder.SetRenderFunc(static (Data d, RasterGraphContext context) => Draw(d, context));
                }
                using (var builder = graph.AddRasterRenderPass<Data>("Sun Rays: Golden Light", out var data))
                {
                    Fill(data, source, 2);
                    builder.UseTexture(source);
                    builder.UseTexture(rays);
                    builder.UseTexture(resources.cameraDepthTexture);
                    builder.SetRenderAttachment(output, 0, AccessFlags.Write);
                    builder.SetRenderFunc(static (Data d, RasterGraphContext context) => Draw(d, context));
                }
                resources.cameraColor = output;
            }

            private void Fill(Data data, TextureHandle source, int index)
            { data.source = source; data.material = material; data.index = index; data.sun = sun; data.color = color; }

            private void Record(RenderGraph graph, string name, TextureHandle source, TextureHandle target, int index)
            {
                using (var builder = graph.AddRasterRenderPass<Data>(name, out var data))
                {
                    Fill(data, source, index);
                    builder.UseTexture(source);
                    builder.SetGlobalTextureAfterPass(source, DepthId);
                    builder.SetRenderAttachment(target, 0, AccessFlags.Write);
                    builder.SetRenderFunc(static (Data d, RasterGraphContext context) => Draw(d, context));
                }
            }
            private static void Draw(Data data, RasterGraphContext context)
            {
                data.material.SetVector("_ForestRaySun", data.sun);
                data.material.SetColor("_ForestRayColor", data.color);
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, data.index);
            }
        }
    }
}
