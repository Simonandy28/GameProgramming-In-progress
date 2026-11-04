from pathlib import Path

# Keep the imported Tenkoku shader untouched. This project-local variant retains
# its original volumetric cloud model but removes jitter that depended on the
# Built-in pipeline temporal reprojection effect.
source = Path('Assets/TENKOKU - DYNAMIC SKY/SHADERS/Tenkoku_cloud_sphere.shader').read_text()
source = source.replace('Shader "TENKOKU/cloud_sphere"', 'Shader "GlowKeeper/Tenkoku Stable Clouds URP"', 1)
old = '''float UVRandom(float2 uv){
            float f = float(dot(float2(_tenkokuNoiseTimer, _tenkokuNoiseTimer), uv));
            return frac(43758.5453 * sin(f));
        }'''
assert old in source
source = source.replace(old, '''float UVRandom(float2 uv){
            // Fixed midpoint sampling is stable without the legacy temporal image effect.
            return 0.5;
        }''', 1)
Path('Assets/GlowKeeper/Shaders/TenkokuStableCloudsURP.shader').write_text(source)
