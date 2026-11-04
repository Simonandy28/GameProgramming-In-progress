# Forest morning and moonlight cycle

This describes the earlier custom controller, which is now disabled. The active Tenkoku sunset and long-night setup is documented in `TENKOKU-SETUP.md`.

The cycle is installed in `Assets/Scenes/GlowKeeper Forest.unity`. Press Play to start from morning every time.

| Time from starting Play | What happens |
| --- | --- |
| 0:00 | Morning lighting and the original cloudy forest sky |
| 0:00 to 0:30 | Smooth fade into night |
| 0:30 to 30:30 | Full moonlit night for 30 minutes |
| 30:30 to 31:00 | Smooth sunrise back to morning |
| 31:00 | Repeat from morning |

The return to morning uses a 30-second sunrise. There is no extra daytime hold: the next fade to night begins when sunrise finishes. All durations use seconds of gameplay. Pausing with `Time.timeScale = 0` or Unity's Pause button pauses the cycle; changing the time scale changes its speed.

## How it works

The preserved, currently disabled `Assets/GlowKeeper/Scripts/Unused Scripts/ForestDayNightCycle.cs` runs one repeating timer when enabled. It calculates a smooth night blend between zero (morning) and one (night). That blend controls all of these together:

- The scene's directional light: warm sunlight changes to cool, shadow-casting moonlight. One light is reused so Idyllic Fantasy Nature's custom vegetation shaders always receive the main light correctly.
- Ambient sky, horizon and ground colors: soft blue fill keeps the forest readable even where direct moonlight is blocked.
- Fog and reflection intensity: distant terrain and water follow the night palette.
- Sky: `Assets/GlowKeeper/Shaders/ForestCycleSky.shader` blends the original cubemap into a dark blue sky with procedural stars and a visible moon aligned with the light.
- Color grading: a separate global Volume blends in a cool night palette and reduces the daytime color boost.
- The demo's 19 sunlight ray effects fade out before full night and return at sunrise.

The timer does not load another scene. Your player, trees, water and gameplay stay in the same scene through the transitions. Runtime sky changes use a private material instance, and stopping Play restores the saved morning appearance.

## Change timing or brightness

Select **Day Night Cycle** in Unity's Hierarchy.

- **Fade To Night Seconds**: 30
- **Night Seconds**: 1800 (30 minutes)
- **Sunrise Seconds**: 30
- **Moon Intensity**: raise it for stronger direct moonlight.
- **Night Sky / Horizon / Ground Ambient**: brighten these to make shadows easier to see.

Make permanent edits outside Play mode, then save the scene. Changes made in Play mode are temporary.

The custom Inspector has **Morning**, **Night** and **Sunrise** buttons during Play mode. These jump to that point on the running timer without changing the saved durations, so you can preview the night without waiting 30 minutes. The Inspector also displays the phase, time and night blend.

## Verification and backup

`AgentScripts/VerifyDayNightCycle.cs` checks ten timestamps: morning, mid-fade, night start, the middle and end of the night hold, sunrise start and midpoint, and repeated-loop boundaries. It also checks that lighting, sky and grading follow the same blend, sunlight rays disappear at night, and the saved morning sky material is preserved. Preview images are under `Assets/Screenshots`.

`AgentScripts/VerifyDayNightClock.cs` also passed four live checks: automatic entry into night, automatic entry into sunrise, automatic wrap to morning, and a stationary timer while gameplay time is paused. The 30-minute hold is verified by its timestamp boundaries; the verification did not wait through an entire 31-minute loop.

The forest scene before this change is backed up in `SetupBackup/DayNightCycle/GlowKeeper Forest.unity`. `AgentScripts/SetupDayNightCycle.cs` records how the light, sky material, grading profile and scene component were wired together.
