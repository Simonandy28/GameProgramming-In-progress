# Unused in the current forest scene

`ForestDayNightCycle.cs` is the old custom day/night controller. Its scene component is disabled; Tenkoku Dynamic Sky and `TenkokuForestSchedule.cs` now control the sky.

The script is retained with its Unity asset identity so the preserved disabled component and its custom Inspector keep their references. This folder organizes inactive scripts; scripts here still compile.

The three scripts in the parent folder are in use:

- `TenkokuForestSchedule.cs`: Tenkoku clock timing and forest lighting.
- `ForestSunRaysFeature.cs`: sunset rays through tree gaps.
- `SuimonoUrpOcean.cs`: Suimono ocean rendering in URP.
