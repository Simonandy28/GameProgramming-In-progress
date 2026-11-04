# Start here: forest controls

The forest has been returned to its original first-person controller. The placed wizard, broken wizard controller reference, and custom wizard Animator controller have been removed from the active project setup. The imported Battle Wizard package's original models, textures, prefabs and animation examples remain available for a later lesson.

## Check the working baseline

1. Open `Assets/Scenes/GlowKeeper Forest.unity`.
2. Press Unity's **Play** button.
3. Select the **Game** tab and click inside the picture so the game receives input.
4. Use **W/A/S/D** or the **arrow keys** to move. Move the mouse to look around.
5. Hold **Left Shift** to run; press **Space** to jump.
6. Press **Escape** to release the mouse. Press Unity's **Play** button again to stop.

Make lasting Inspector changes after stopping Play mode. Changes made during Play are normally reverted when Play stops.

## What each part does

| Scene object/component | Purpose |
| --- | --- |
| `Controls/Controller` > `CharacterController` | Capsule collision and contact with the ground. |
| `Controls/Controller` > `PlayerMovement` | Keyboard movement, running, gravity and jumping. |
| `Controls/Controller/Main Camera` > `CameraMovement` | Mouse look and cursor capture. Its controller reference points to `Controller`. |
| `Controls/VegetationBendControl` | Grass and bush bending. Both components reference `Controller`. |

This baseline has no wizard model or character Animator in the forest. Movement and animation are separate systems: first confirm movement and the camera, then place one model, and only then connect its Animator. Crouching and crawling can be a later step after the basic character works.

## What caused the broken state

The original movement and mouse-look components had been disabled during the wizard setup. The camera had been detached from the player and repositioned. Deleting wizard setup objects left a missing controller script and two vegetation effects with no bend origin.

The original components have been enabled, the camera hierarchy and starting view restored, and the missing references repaired. The vegetation script also tolerates an unassigned origin without repeatedly throwing exceptions.

The pre-recovery scene copies and removed custom Animator controller are kept outside `Assets`, under `SetupBackup/ForestControlReset`.
