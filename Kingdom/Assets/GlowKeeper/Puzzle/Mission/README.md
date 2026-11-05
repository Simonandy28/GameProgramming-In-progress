# The Whispering Fireflies

Open `Assets/Glowkeeper_Update.unity`. The mission and its Inspector fields are
already set up.

## The small idea behind the code

`FirstPuzzleMission.cs` keeps one variable called `stage`. It checks what has
already happened in the puzzle. For example, `detector.swarm.IsAtHand` becomes
true when the fireflies actually reach the hand. The mission then switches to
`FollowSwarm` and changes the objective. The subtitle timer only fades the text.

- `FireflyHandAttraction.cs` exposes `CanAttract` so the HUD uses the same
  distance check as the E interaction.
- `FireflyStoneDetector.cs` exposes when E can awaken the stone. Its existing
  smoothed signal drives the meter and the buzz.
- `PlayerSwordPuzzle.cs` exposes when the sword can be claimed. An E press
  while holding the sword starts the storage timer. The pickup press equips it.

## Inspector wiring

Select **Whispering Fireflies HUD** in the Hierarchy. Its **First Puzzle
Mission** component has these fields.

| Field | Drag this into it |
| --- | --- |
| Detector | `BeeSwarm` with Firefly Stone Detector |
| Player | `PlayerRoot` with Player Sword Puzzle |
| Objective | `Quest Objective > Objective Text` |
| Prompt | `Interaction Hint > Hint Text` |
| Subtitle | `Poetic Subtitle > Subtitle Text` |
| Subtitle Group | `Poetic Subtitle` with Canvas Group |
| Chimes | The Audio Source on `Whispering Fireflies HUD` |
| Awakening Chime | `StoneAwakens.wav` |
| Victory Chime | `FirstBladeChime.wav` |
| Objective Buzz Volume | How loud the buzz needs to be for the humming objective to appear |

On `BeeSwarm`, **Detector Meter** points to the Slider on **Whisper of the
Swarm**. The Canvas Scaler uses a 1920 × 1080 reference size. The meter itself
is 208 × 7 at that size. Its fill is antique gold over a dark brown track.
The typeface is EB Garamond. Its SIL Open Font License is included alongside
the font.

## Try it in Play Mode

1. Press **Play**. Approach the glowing firefly plant and listen for the hum.
   The opening clue and objective appear when the buzz gets louder. The
   objective hides when you move away and comes back when you return.
2. Within **3 metres** of the plant, press **E**. Watch the absorption spell.
   The detector appears once the fireflies reach the right hand.
3. Turn the character with the movement keys and walk toward the stone near
   the statue. The buzz and meter get stronger as you face it and approach.
4. Within **3.5 metres** of the stone, press **E**. The meter disappears. The
   release spell sends the fireflies over, then a chime plays and the stone
   starts glowing blue.
5. Face the stone from close by and **left-click** once. Let the full combo
   play. Its three hits crack the stone, and the sword drops before hovering.
6. Step within **2.6 metres** of the sword and tap **E**. The sword goes into
   the right hand and the victory chime and messages play.
7. Release **E**, then hold it again for **one second**. On `PlayerRoot`,
   expand **Player Sword Puzzle > Stored Blades** to see the sword in slot 0.

Stop Play to reset the mission.
