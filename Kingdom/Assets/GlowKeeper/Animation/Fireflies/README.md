# Animator fireflies

Open `Assets/Glowkeeper_Update.unity` and expand `BeeSwarm` in the Hierarchy.
Select `Firefly_1`, `Firefly_2`, or `Firefly_3`: each has an **Animator** with its
own `Firefly_N_FigureEight.controller` assigned.

Press **Play**. Each Animator's default **Figure Eight Flight** state loops
automatically. Figure-eight flight is controlled by these Animator clips.

Each `Firefly_N` is a holder containing your existing `Firefly_Fred` model prefab.
The clips animate only Fred's local Position, preserving the model's scale,
rotation, materials, and other components. The paths follow the `BeeSwarm`
parent when it moves or rotates. Keep the holder and anchor scales at one.

To edit the flight:

1. Select a `Firefly_N` holder, then open **Window > Animation > Animator**.
2. Select **Figure Eight Flight** to change its **Speed**. The three defaults
   are 1.00, 1.09, and 1.18. Each underlying clip lasts six seconds.
3. Open **Window > Animation > Animation** to edit the matching clip's
   `Firefly_Fred > Transform > Position` curves. X and Z form the figure eight;
   Y adds a small bob and separates the three heights.
4. Keep **Loop Time** enabled on each clip. Keep first/last key values and their
   tangents matched to preserve a smooth loop.

The three clips already start one third of a loop apart and use different
radii and heights. Make lasting edits in Edit mode, then save the scene/assets.

`FireflyHandAttraction` now moves the shared `BeeSwarm` anchor toward the hand
when E is pressed within 3 metres. These flight Animators continue playing.
See `Assets/GlowKeeper/Scripts/FireflyHandAttraction.md` for interaction setup.
Stone travel and sound are outside the current mechanic.
