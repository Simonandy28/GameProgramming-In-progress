# Forest world baseline - 4 November 2026

Current scene: `Assets/Glowkeeper_Update.unity`. It is open in the editor and is the enabled build entry scene. The earlier forest and sample scenes remain available but are disabled in the build list.

## Preserved

All surviving scene objects retain their saved positions, rotations, scales, and parents. This includes terrain, vegetation, houses, statues, water/ocean rendering, five WaterVolume swimming triggers, the avatar model and Animator controller, third-person movement/camera, and firefly/lamp interactions. No avatar animation assets or input bindings were changed.

Tenkoku keeps the saved 3-minute sunset, 30-minute night, 2-minute sunrise, and 2-minute daylight schedule, repeating every 37 minutes.

## Removed

Zombie scene instances, ZombieAI, PlayerDeath (including damage, health display, death/game-over UI, and timeScale freeze), zombie navigation surfaces/exclusion helpers, zombie model/textures/animation assets, and the unreferenced zombie navigation data were removed from Assets. The old disabled ForestDayNightCycle hierarchy/script/editor, old sky material/shader, and moonlight grading profile were also removed. The active forest profile's ColorAdjustments, WhiteBalance, ChannelMixer, SplitToning, and ShadowsMidtonesHighlights overrides are disabled. Other rendering effects remain as configured.

Both forest scene variants and the two recovery scenes were checked and cleaned so they do not retain missing scripts or references to the removed assets.

No independent 30-second gameplay cutoff was found in the project scripts. The removed PlayerDeath code was the gameplay freeze mechanism; the obsolete 30-second cycle was already disabled. Runtime verification reached 102.8848 seconds of gameplay while avatar animation and the sky clock continued.

## Verification

- Unity compilation succeeded; all four affected scenes have zero missing-script components.
- Runtime input activated walking, running, water entry, and swimming-movement Animator parameters. An initial water test moved through a small volume before sampling; the follow-up physics-frame test confirmed water entry and swimming.
- The current scene retains 5 water volumes, 29 objects with House in their names, and 17 with Statue in their names.
- Scene serialization comparison against the backup found no changes to surviving transform coordinates or parents. The active schedule only lost its moonlight grading reference; timing values were unchanged.
- Preview: `AgentScripts/world-cleanup-preview.png`.
- Reports: `AgentScripts/world-cleanup-playcheck.txt`, `AgentScripts/world-cleanup-final-check.txt`, and `AgentScripts/world-cleanup-scene-comparison.json`.

Original scenes, settings, documentation, and removed assets are preserved under `SetupBackup/WorldCleanup-2026-11-04`, outside Unity's Assets directory. Stopping Play restores the saved starting location; test input and runtime relocations were not saved.
