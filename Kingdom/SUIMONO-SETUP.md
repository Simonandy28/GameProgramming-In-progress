# Island ocean

`Assets/Scenes/GlowKeeper Forest.unity` now has **Island Ocean - Suimono**, covering a 2400 × 2400 metre area at the original ocean's sea level of 3 metres. The former `Ocean` renderer is disabled. The inland pond is unchanged.

The imported Suimono 2 package targets Unity's Built-in renderer. Its unmodified surface shaders and camera image effects do not support this project's URP renderer. The scene therefore uses a focused URP bridge:

- Original `SUIMONO_Module` and `SUIMONO_Surface` prefab instances retain the package references and settings.
- `SuimonoUrpOcean` reads the surface's flow direction/speed, wave heights, turbulence, shallow/deep colors, and foam setting.
- The project-local `Suimono Ocean URP` shader uses Suimono's three normal/wave textures, foam texture, and calm/turbulent/rolling wave-height formula.
- URP depth supplies shoreline foam and soft water edges; the camera's opaque texture supplies refraction. URP lighting, shadows and environment reflections follow Tenkoku's current sun/moon.

Select **Island Ocean - Suimono** to adjust the bridge's ripple strength, foam width and wave world size. Expand **Suimono Island Ocean Surface** to edit the original Suimono settings. Its legacy behaviour remains disabled because the bridge handles URP rendering. The legacy underwater image effects, caustic cameras, tessellation and interactive wakes are not enabled by this integration.

The old scene was backed up under `SetupBackup/ForestRevision` before these edits. `AgentScripts/ForestRevisionTools.cs` records the scene installation.
