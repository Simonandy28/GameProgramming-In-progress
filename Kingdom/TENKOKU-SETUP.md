# Tenkoku sky in GlowKeeper Forest

The active sky in `Assets/Glowkeeper_Update.unity` is **Tenkoku DynamicSky**. The obsolete **Forest Day Night Cycle** hierarchy and script have been removed. The current full-day schedule is described below. The moonlight grading volume and forest color-grading overrides have also been removed or disabled.

Tenkoku calculates the sun, moon, lunar phase, stars and sky from its date, time, latitude and longitude. `TenkokuForestSchedule` only supplies the gameplay clock and adapts ambient lighting and fog to URP. It does not position, rotate or animate the moon.

## Timing

Each Play starts at dawn (06:00), followed by sunrise, morning, noon, afternoon, sunset and night. A complete loop takes 47 gameplay minutes.

| Gameplay time | Phase | Tenkoku clock |
| --- | --- | --- |
| 0:00-2:00 | Sunrise | 06:00-08:00 |
| 2:00-6:48 | Morning | 08:00-12:00 |
| 6:48-9:12 | Noon | 12:00-14:00 |
| 9:12-14:00 | Afternoon | 14:00-18:00 |
| 14:00-17:00 | Sunset | 18:00-20:00 |
| 17:00-47:00 | Night | 20:00-06:00 next day |

The 2-minute sunrise, 3-minute sunset and 30-minute night are retained. Daylight is now 12 minutes. The date advances every complete loop; the initial date is 21 March 2019, latitude 35 degrees, longitude zero. Change the four phase lengths on **Tenkoku Forest Schedule**. **Sunrise**, **Morning**, **Noon**, **Afternoon**, **Sunset** and **Night** buttons preview this clock. See `DAY-NIGHT-CYCLE.md` for the current timing and verification details.

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


