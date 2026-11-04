from pathlib import Path

source = Path('Assets/TENKOKU - DYNAMIC SKY/SHADERS/Tenkoku_sky_legacy.shader').read_text()
source = source.replace('Shader "TENKOKU/Tenkoku_Sky_Legacy"', 'Shader "GlowKeeper/Tenkoku Orange Sunset URP"', 1)
source = source.replace('float _Tenkoku_SkyBright;', 'float _GlowKeeperSunsetAmount;\nfloat _Tenkoku_SkyBright;', 1)
old = 'return half4(col,1.0);'
assert old in source
source = source.replace(old, '''// Broad orange dusk glow while retaining the Tenkoku atmosphere model.
            float3 viewDirection=normalize(-IN.rayDir.xyz);
            float horizonBand=exp(-max(0,viewDirection.y)*3.8);
            float sunGlow=pow(saturate(dot(viewDirection,normalize(Tenkoku_Vec_SunFwd.xyz))),5);
            float3 warmSky=lerp(float3(.72,.095,.025),float3(1.3,.43,.06),sunGlow);
            col=lerp(col,warmSky,saturate(_GlowKeeperSunsetAmount*horizonBand*(.65+.3*sunGlow)));
            return half4(col,1.0);''', 1)
Path('Assets/GlowKeeper/Shaders/TenkokuOrangeSunsetURP.shader').write_text(source)

path = Path('Assets/GlowKeeper/Shaders/TenkokuStableCloudsURP.shader')
cloud = path.read_text()
if '_GlowKeeperSunsetAmount' not in cloud:
    cloud = cloud.replace('float _Tenkoku_UseElek;', 'float _Tenkoku_UseElek;\nfloat _GlowKeeperSunsetAmount;', 1)
    old = 'return half4(retCol, 1-alph);'
    assert old in cloud
    cloud = cloud.replace(old, '''retCol=lerp(retCol,retCol*float3(1.5,.55,.12)+float3(.22,.055,.005),saturate(_GlowKeeperSunsetAmount)*.65);
            return half4(retCol, 1-alph);''', 1)
    path.write_text(cloud)
