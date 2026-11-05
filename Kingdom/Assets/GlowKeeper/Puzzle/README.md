# Firefly detector: Plant -> Hand -> Stone

Everything is assigned in `Assets/Glowkeeper_Update.unity`.
The rock selected in front of `Statue_OnRock_Perch_02` is now named `MagicStone`.
Its position is approximately **(196.74, 15.83, 206.82)**. Its original shape,
position, material and collider are preserved.

## Play the puzzle

1. Press **Play** and approach the firefly plant. Its buzzing grows louder as
   you get closer and becomes quiet when you move away (14m range by default).
   Each firefly now has a small glowing tail tip and a point light on its tail.
   Go within 3 metres of the plant.
2. Press **E**. The player casts and the three fireflies fly to the right hand.
3. The plant buzz fades out when they leave. Once they arrive at your hand,
   a cyan **FIREFLY DETECTOR** bar appears and directional detector buzzing begins.
4. Turn the **character** toward the stone and walk closer. The bar fills, and
   the buzz grows louder and higher. Turning only the camera does not change
   the character's facing direction.
5. Within **3.5 metres** of the stone's center, press **E** again.
   The supplied **Standing 2H Magic Area Attack 02** gesture plays once while
   the fireflies leave the hand. This is separate from the first plant-attraction spell.
6. The fireflies leave the hand and fly above the selected rock. The detector
   turns off. When the fireflies arrive, the stone starts glowing faintly,
   then fades up to vivid magical blue over 3 seconds.
7. Walk away: the fireflies keep flying above the stone. Stop and restart Play
   to reset the puzzle.

## The code, in small pieces

There are two scripts on **BeeSwarm**. The three original flight Animators
still draw the figure eights. Neither script creates another flight animation.

### 1. FireflyHandAttraction.cs: move the shared anchor

This existing script was extended with a second destination.

Think of `BeeSwarm` as a tray holding three flying insects. We move the tray;
each insect keeps doing its own little dance on it.

- `IsAtHand` means the tray has reached the hand.
- `FlyToStone(target)` remembers the gap from the tray to the stone target.
- `MoveTowards` makes that gap smaller every frame.
- `IsAtStone` becomes true only when the gap reaches zero.

The script keeps the models' scales unchanged. It changes the size of their
flight paths instead: small near the hand, larger above the stone.
**Movement Speed** on this script is the swarm travel speed, initially **5**.

### 2. FireflyStoneDetector.cs: read the signal and show it

The important idea is just:

```csharp
float distance = Vector3.Distance(player.position, stone.position);
float proximity = 1f - Mathf.Clamp01(distance / detectionRange);
float facing = Mathf.Clamp01(Vector3.Dot(player.forward, direction.normalized));
float wantedStrength = facing * proximity;
```

| Name | Plain meaning |
| --- | --- |
| `distance` | How far away is the stone? |
| `proximity` | A number near 1 means close; 0 means outside the detector range. |
| `facing` | 1 means looking straight toward it; 0 means sideways or away. |
| `wantedStrength` | Strong only when both facing and proximity are good. |

For example, `facing = 1` and `proximity = 0.8` gives a signal of `0.8`.
Facing away makes the target signal `0`, even when nearby.

The real script removes the direction's Y value so terrain height does not
make you look down. It uses `Mathf.Lerp` to ease toward the target signal.
That same smoothed number sets the bar value, audio volume and audio pitch.

The last two rules are simple:

```csharp
// E near the stone asks the existing swarm script to change destinations.
swarm.FlyToStone(stoneBeeTarget);

// The blue effect starts after arrival, never when E is first pressed.
if (swarm.IsAtStone) blueGlow.SetActive(true);
```

These are teaching snippets; the complete scripts include the range/input
checks and Inspector fields. Edit the existing files instead of pasting a
second copy of either class into the project.

## Inspector assignments

All fields are already connected. If reconnecting them, use:

| Component / field | Drag this object |
| --- | --- |
| `BeeSwarm` / Firefly Stone Detector / Swarm | `BeeSwarm`, which has FireflyHandAttraction |
| Magic Stone | The renamed rock `MagicStone` in front of the statue |
| Stone Bee Target | `MagicStone > StoneBeeTarget` |
| Buzzing | Audio Source on `BeeSwarm` |
| Detector Meter | `FireflyDetectorCanvas > FireflyDetectorMeter` (Slider) |
| Blue Glow | `MagicStone > MagicStoneBlueGlow` |
| FireflyHandAttraction / Player | `PlayerRoot` |
| Bee Hand Target | Existing `Hand.R > BeeHandTarget` |
| Player Animator | Animator on `Axe Warrior Avatar` |
| Interact Action | `PlayerControls: Player/Interact` (E) |
| Fireflies, Size 3 | Each `Firefly_Fred` model inside its `Firefly_1/2/3` holder |

Adjust **Detection Range**, **Close Activation Distance**, **Signal Smoothing**,
**Min/Max Volume**, and **Min/Max Pitch** on Firefly Stone Detector.
Adjust **Movement Speed**, **Hand Formation Size**, and **Stone Formation Size**
on Firefly Hand Attraction. Keep all three flight Animators enabled with
**Culling Mode = Always Animate** and **Update Mode = Normal**.

Adjust **Plant Buzz Range** and **Plant Max Volume** on Firefly Stone Detector
to change how far the plant can be heard. **Plant Buzz** points to the Audio
Source on the stationary `FireflyPlantBuzz` object. If relocating the plant
and `BeeSwarm` in Edit mode, move `FireflyPlantBuzz` to the new plant position too.

Each `Firefly_Fred` has a `TailGlow` point light under its `Bone.003` tail bone.
These follow each insect's existing animation and use no light-animation script.
The tiny emissive `GlowingTailTip` makes the source itself visible. Adjust each
light's **Intensity** or **Range** in the Inspector if needed.

`MagicStone > MagicStoneBlueGlow` now has **MagicStoneGlow.cs** attached.
Its **Stone Surface** is `GlowingStoneSurface` and its **Blue Light** is
`BlueMagicLight`. These references are already assigned. Adjust **Fade Duration**
(3 seconds), **Glow Color** (HDR blue), or **Light Brightness** (9) there.

The fade code is deliberately simple:

```csharp
brightness = Mathf.MoveTowards(brightness, 1f, Time.deltaTime / fadeDuration);
blueLight.intensity = lightBrightness * brightness;
```

`brightness` starts at 0 when the bees arrive, grows gradually to 1, and
controls the surface's blue emission and transparency as well as the light.
The transparent surface starts invisible, so there is no instant blue surface
pop. The script changes its own material copy, preserving the saved starting
material for the next Play session.

The player's existing **Firefly Casting** Animator layer now also has a
**Release Fireflies** state, using the imported two-handed animation. The
`ReleaseFireflies` trigger starts it when `FlyToStone` succeeds. It can interrupt
the initial attraction gesture if you reach the stone quickly, then automatically
returns to **Ready** after the clip finishes (about 3.27 seconds). The upper-body
mask lets normal walking/running continue. The release is not looping or tied to
every E press; it plays only when sending the bees from the hand to the stone.

The close interaction checks distance to the rock's center. The default 3.5m
allows interaction beside the rock without walking into its collider.
`StoneBeeTarget` is above the top surface so the flights stay visible.

## Created scene objects and assets

- `FireflyDetectorCanvas`: simple screen-space UI with a non-interactive Slider.
- `StoneBeeTarget`: a child of the chosen rock, above its surface.
- `MagicStoneBlueGlow`: initially inactive; a blue emissive copy of the rock's
  surface and one blue point light, faded in by MagicStoneGlow.cs. It has no
  additional collider.
- `FireflyPlantBuzz`: stationary plant audio, controlled by player distance.
- Three `TailGlow` point lights and luminous tips, one on each firefly.
- `FireflyTailGlow.mat`: yellow-green emission material for the tiny tail tips.
- `MagicStoneBlueGlow.mat`: URP blue emission material for that effect only.
- `FireflyBuzz.wav`: an original, synthetic, seamless buzzing placeholder.
  Replace the Audio Source's Clip if you later get a recorded insect sound.

The rock's original shader has no emission property, so the separate glow
effect provides a reliable bright blue activation without changing other rocks.
The next step now adds stone cracking and sword pickup/storage. See
`Assets/GlowKeeper/Puzzle/StoneSword/README.md` for the full beginner walkthrough.
