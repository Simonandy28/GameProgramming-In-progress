# Idyllic Fantasy Nature in GlowKeeper

Open `Assets/Scenes/GlowKeeper Forest.unity` in Unity. This is a working copy of the publisher's complete forest demo, with the original trees, flowers, terrain, lake, sky and effects. `SampleScene` and the original package demo are preserved.

Press Play, then click inside the Game view to look around.

- WASD or arrow keys: walk
- Mouse: look
- Shift: run
- Space: jump
- Escape: release the mouse
- Unity's Play button: stop

Project configuration:

- Unity 6000.4.5f1 with the already installed URP 17.4.0.
- Dedicated `Assets/GlowKeeper/ForestSettings/ForestPipeline.asset` and `ForestRenderer.asset` using Forward rendering, camera depth and opaque textures, and 4x MSAA.
- Gamma color space to match the publisher's documented palette.
- Copies of terrain data and post processing settings in `Assets/GlowKeeper/ForestSettings` for customization.
- Forest scene placed first in the build scene list.

Compatibility changes:

- The vegetation graph's `_MAIN_LIGHT_SHADOWS` and `_MAIN_LIGHT_SHADOWS_CASCADE` keywords use Predefined mode. URP already generates these shader variants, so generating them again caused Unity 6 shader errors.
- The package's movement, camera and Escape scripts support the active Input System. Legacy input remains available behind compile directives.
- Camera rotation starts at its saved orientation; click captures the mouse and Escape releases it.

To change colors, expand the imported asset's Materials folder and select a foliage material. Use its Custom Color, Top Color, Bottom Color and Blend Height controls. Duplicate a material before making a separate color variant.

The files changed during setup were backed up under `SetupBackup/IdyllicFantasyNature` before editing. `AgentScripts/SetupForest.cs` records the scene setup operation; `AgentScripts/FixForestShader.py` records the shader compatibility patch. Reimporting the publisher's package can overwrite the compatibility changes.

The active sky now uses Tenkoku Dynamic Sky, beginning at sunset with an extended night. See `TENKOKU-SETUP.md` for timing and controls. The earlier custom morning/night controller is preserved but disabled; `DAY-NIGHT-CYCLE.md` describes that earlier setup.

The latest scene revision adds four houses and six concrete statues. Each new statue rests on a large rock placed on a grassy hill; additions are grouped under **Hilltop Settlement Additions**. Bark, rock and road changes use scene-specific copies in `Assets/GlowKeeper/ForestSettings/LightSurfaces`, preserving the original texture maps and imported assets.

The surrounding sea now uses Suimono assets through a project-local URP surface bridge. See `SUIMONO-SETUP.md` for supported water features and controls.
