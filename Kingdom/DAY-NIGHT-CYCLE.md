# Full day/night cycle

Open `Assets/Glowkeeper_Update.unity`. Each Play begins at dawn and runs continuously through sunrise, morning, noon, afternoon, sunset and night. The cycle repeats after 47 gameplay minutes; it never ends or freezes gameplay.

| Time since Play | Phase | Sky clock |
| --- | --- | --- |
| 0:00-2:00 | Sunrise | 06:00-08:00 |
| 2:00-6:48 | Morning | 08:00-12:00 |
| 6:48-9:12 | Noon | 12:00-14:00 |
| 9:12-14:00 | Afternoon | 14:00-18:00 |
| 14:00-17:00 | Sunset | 18:00-20:00 |
| 17:00-47:00 | Night | 20:00-06:00 next day |
| 47:00 | Repeat at sunrise | Next calendar day |

The existing 2-minute sunrise, 3-minute sunset and 30-minute night durations are retained. Daylight is expanded from 2 to 12 minutes so morning, noon and afternoon have time to be seen. The clock starts at 06:00 rather than sunset.

Select `Tenkoku DynamicSky` in the Hierarchy and edit `Tenkoku Forest Schedule` to change the four phase durations. Its preview buttons jump to Sunrise, Morning, Noon, Afternoon, Sunset or Night. The Inspector displays the current phase during Play.

The same native Tenkoku sun/moon, shadows, stars, clouds, ambient lighting and fog follow this clock. Moon motion remains astronomical. The removed color-grading filters remain removed. The avatar, armor, controls, water and world are unchanged by this sky revision.

Verification reports: `AgentScripts/full-day-cycle-math-check.txt` and `AgentScripts/full-day-cycle-native-check.txt`. Phase previews are under `AgentScripts/FullDayPreviews`. These checks evaluate the phase boundaries and preview native sky states; they do not wait through all 47 minutes in real time.

Pre-change scenes, scripts and sky documentation are backed up in `SetupBackup/FullDayCycle-2026-11-04`.

