# The sword in the magic stone

Everything is connected in `Assets/Glowkeeper_Update.unity`.
The sword is **Sword15_Frost** from your imported FreeSwords pack. Its handle
sticks out above MagicStone, with half the sword below the stone's top surface.
It leans at an angle, with its handle exposed. It has its own URP material and
a small blue light; the imported pack is unchanged.

## Play it

1. Collect the fireflies at the plant with **E**.
2. Follow the buzzing/bar to MagicStone, then press **E** to send the bees there.
3. **Left-click anywhere** to attack in the direction you are facing. Clicking
   again restarts the combo. You can still walk or run during the attack.
   To damage MagicStone, stand close to it and face it after the bees activate it.
4. Your supplied **One Hand Club Combo** plays once. Its three hits add cracks,
   play impact sounds, and finally break the rock into small pieces.
5. The sword falls with real gravity, then lifts into a gentle spinning hover.
6. Go near the hovering sword and **tap E** to hold it in your right hand.
7. **Hold E for one second** to put it into `storedBlades`.
8. Select `PlayerRoot` during Play and expand **Player Sword Puzzle > Stored
   Blades** to see the stored sword reference.

The puzzle resets when you stop Play. The array stores the sword for the
current Play session. Release E after picking up the sword, then press and
hold it again to store the sword.

## Small scripts, one job each

| Script | Attached to | Its job |
| --- | --- | --- |
| `CrackableMagicStone.cs` | `MagicStone` | Count hits, show cracks, play sounds, show rubble, release the sword |
| `FloatingSword.cs` | `FrostSwordTreasure` | Fall, hover, move to hand, hide when stored |
| `PlayerSwordPuzzle.cs` | `PlayerRoot` | Read left-click/E and keep the stored-blades array |
| `StoneStrikeEvents.cs` | `Axe Warrior Avatar` | Receive animation hit events and keep hands outside the rock |
| `SwordHandGrip.cs` | `Axe Warrior Avatar` | Keep the right-hand fingers closed around the held sword |

The existing player movement script is not edited or paused. The player's existing casting Animator
layer has a **Strike Magic Stone** state, played directly by the left-click code.
The original bee attraction/release animations are retained and still use E
at their corresponding interaction points. Left-click attacks are independent
of those interaction points. `Animator.Play` restarts the attack state directly.

## The code in very plain steps

### Count hits

```csharp
Hits++;
if (Hits >= hitsToBreak) BreakStone();
```

`++` means "add one." The first two hits show more cracks. The third breaks
the rock. The animation sends `HitStone` events at 0.8, 1.75 and 2.65 seconds,
so the sounds and cracks happen during the swings, not immediately on click.

### Let gravity work

```csharp
body.isKinematic = false;
body.useGravity = true;
```

"Not kinematic" means Unity physics can move the sword. After a short fall and
ground contact, the script makes it kinematic again and uses a small sine wave
for the magic bobbing. A short timeout also starts the hover if it misses the ground.

### Put it in the hand

```csharp
transform.SetParent(hand, true);
transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
```

The sword becomes a child of `SwordHandTarget`, which follows the right-hand bone.
Its world size is preserved. Its collider is disabled while held so it cannot
push the player around.

The model's edited stone-placement offset and rotation are cleared **only while
held**. `Model Grip Point` identifies the handle center inside the model, and
the script puts that point exactly at `SwordHandTarget`. The current model's
smaller **0.45** scale and its authored stone placement are preserved.
The palm target follows the right-hand bone and lines the shaft across the
fingers. `SwordHandGrip` applies a closed finger pose taken from your existing
one-handed attack clip, without changing the arm animation. Storing the sword
releases that finger pose again.

### Remember it in an array

```csharp
storedBlades[i] = sword.gameObject;
sword.Store(storage);
```

An array is a row of boxes. `i` is a box number, starting at zero. The script
finds the first empty box and puts the sword reference in it. It then hides the
sword under `PlayerRoot > StoredBlades`. It is kept, not destroyed. If all eight
boxes are full, it stays in your hand.

### Keep hands outside

`StoneStrikeEvents` checks a line from each upper arm to its animated hand.
If that line enters the stone, it moves the hand target just outside the surface
using Unity's built-in hand IK. A second check protects hands when a swing starts
very close to the stone. Damage checks the player's range and facing direction
at each animation hit event, so swinging elsewhere never cracks the rock remotely.

## Inspector references

These are already assigned. To reconnect manually:

- **PlayerRoot / PlayerSwordPuzzle**: Stone = `MagicStone`; Sword =
  `FrostSwordTreasure`; Strike Events and Animator = `Axe Warrior Avatar`;
  Attack Action = `PlayerControls: Player/Attack` (left mouse);
  Interact Action = `PlayerControls: Player/Interact` (E);
  Sword Hand Target = `Hand.R > SwordHandTarget`; Storage = `PlayerRoot > StoredBlades`.
- **Axe Warrior Avatar / StoneStrikeEvents**: Stone = `MagicStone`;
  Animator = the Animator on this same avatar.
- **FrostSwordTreasure / FloatingSword**: Body = its Rigidbody;
  Sword Collider = its BoxCollider; Sword Visual = its `Sword15_Frost` child;
  Model Grip Point = `(0, -0.05, 0)`; Sword Light = `FrostSwordLight`;
  Held Light Intensity = `0.4`, keeping the hand visible. The visual is the
  existing `Sword15_Frost` prefab, at your edited size.
- **Axe Warrior Avatar / SwordHandGrip**: Sword = `FrostSwordTreasure`;
  the 15 right-hand finger joints and their closed rotations are already calibrated.
- **MagicStone / CrackableMagicStone**: Detector = `BeeSwarm`'s detector;
  Sword = `FrostSwordTreasure`; Solid Collider/Stone LOD = the original stone
  components; Stone Renderers = original `LOD0/1/2`; Crack Stages = `CrackStage1/2`;
  Rubble = `BrokenStoneRubble`; Impact Audio = Audio Source on MagicStone;
  sounds = `StoneImpact.wav` and `StoneBreak.wav` in `Assets/GlowKeeper/Puzzle`.

Useful settings are **Attack Range**, **Pickup Range**, **Store Hold Time**, and
the **Stored Blades** array size on PlayerRoot. **Hand Clearance** is on the
avatar's StoneStrikeEvents. Keep Combo Duration equal to the imported clip's
3.633-second length. The rock uses 3 hits to match the three animation events.

For small grip adjustments, edit the local Position/Rotation of
`Hand.R > SwordHandTarget`. Adjust `Model Grip Point` on FloatingSword if using
a different sword model with a different handle pivot. Model offsets used to
place the sword in the stone no longer create a gap at pickup.

The crack/break sounds are original synthesized prototype effects. Replace the
two AudioClip fields if you later obtain recorded stone sounds. Rubble uses
smaller copies of the existing stone mesh, with simple physics colliders.
