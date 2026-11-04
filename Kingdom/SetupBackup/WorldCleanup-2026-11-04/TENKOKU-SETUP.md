# Tenkoku sky in GlowKeeper Forest

The active sky in `Assets/Scenes/GlowKeeper Forest.unity` is **Tenkoku DynamicSky**. The previous **Forest Day Night Cycle** component and its separate directional light are disabled.

Tenkoku calculates the sun, moon, lunar phase, stars and sky from its date, time, latitude and longitude. `TenkokuForestSchedule` only supplies the gameplay clock and adapts ambient lighting and fog to URP. It does not position, rotate or animate the moon.

## Timing

Each Play starts at sunset. The saved phase durations are:

| Gameplay time | Phase | Tenkoku clock |
| --- | --- | --- |
| 0:00 to 3:00 | Sunset | 18:30 to 20:30 |
| 3:00 to 33:00 | Long night | 20:30 to 06:30 next day |
| 33:00 to 35:00 | Sunrise | 06:30 to 08:30 |
| 35:00 to 37:00 | Daylight | 08:30 to 18:30 |

The date advances each loop. The initial date is 21 March 2019, latitude 35 degrees, longitude zero. Change the four phase lengths on **Tenkoku Forest Schedule**. The **Sunset**, **Midnight**, and **Sunrise** buttons preview the same Tenkoku clock.

## Sunset rays and exposure

**Forest Sunset Sun Rays** on `ForestRenderer.asset` reads Tenkoku's actual sunlight direction. A depth mask identifies gaps between trees; radial scattering produces defined shafts that fade as the sun drops below the horizon or leaves the view. The former custom cycle is not a dependency of this effect.

Adjust **Sun Rays Strength** and **Sunset Orange Strength** on the schedule. The sun disc's material uses **Sun Disc Brightness** (0.65), rather than a fixed overbright multiplier. Tenkoku's sun brightness is 0.7 and the forest sunlight limit is 0.8. Softer shadow strength and ambient ground fill preserve the lighter bark, stone and road palette.

## Native moon

The previous camera-facing moon arc has been removed from the runtime code and Inspector. Tenkoku's moon is in its native astronomical mode (`moonTypeIndex = 0`), using its original local position and orbital parent rotation. Turning the camera does not move the moon. Its location changes naturally with the sky clock and date, so it may be behind the starting camera or below the horizon.

The URP moon shader preserves Tenkoku's lunar phase and original moon texture. Modest earthshine and texture brightness retain crater detail. Its Tenkoku size is 0.027. No forced full-moon phase or custom moon positioning remains.

## URP compatibility

The imported Tenkoku version targets the Built-in renderer. The project retains its native celestial calculations and uses project-local URP sun, moon, cloud and planet materials. Built-in fog, sun shafts, temporal reprojection and helper cameras are disabled. The URP depth-based rays feature replaces the Built-in sun-shaft image effect.

Suimono's surrounding ocean is described in `SUIMONO-SETUP.md`.

## Verification and backups

`AgentScripts/ForestRevisionTools.cs` records the current setup and checks the native moon at ten timeline checkpoints, active sky components, renderer feature and shader diagnostics. Its Play mode check captures sunset with rays on/off, night, the actual moon direction, morning and the ocean. Results are saved in `AgentScripts/forest-revision-playcheck-results.txt`.

Current previews use the `Assets/Screenshots/Final-` prefix. Older screenshots and earlier setup scripts record historical configurations and are not current moon-placement references.

The scene and affected sky sources were backed up in `SetupBackup/ForestRevision` before this revision.