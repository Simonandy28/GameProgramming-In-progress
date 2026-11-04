# Day/night cycle

The active sky uses Tenkoku and `TenkokuForestSchedule` in `Assets/Glowkeeper_Update.unity`. Its existing timings are unchanged:

| Phase | Gameplay duration |
| --- | --- |
| Sunset | 3 minutes |
| Night | 30 minutes |
| Sunrise | 2 minutes |
| Daylight | 2 minutes |

The cycle repeats after 37 minutes and advances the astronomical date. It does not end gameplay. See `TENKOKU-SETUP.md` for sky details.

The unused earlier controller with 30-second sunset/sunrise phases, its disabled hierarchy, and its moonlight grading filter were removed. The active forest profile's color-grading overrides are disabled; sky lighting, fog, sun/moon motion, ocean rendering, and sun rays remain.

The previous documentation and removed assets are preserved outside Unity's Assets directory in `SetupBackup/WorldCleanup-2026-11-04`.
