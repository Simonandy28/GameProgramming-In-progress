# Current forest controls

Open `Assets/Glowkeeper_Update.unity` and press Play. The player is the clothed axe warrior with chest and shoulder armor, beige linen sleeves, bracers, trousers, knee guards, boots, and both axes.

- W/A/S/D: move relative to the camera.
- Left Shift: run.
- Space: jump on land; ascend while swimming.
- Left Ctrl: descend while swimming.
- Mouse: orbit the third-person camera.
- Escape: release the cursor; click the Game view to capture it again.

The magic staff and its X/E firefly collection and lamp-deposit gameplay have been removed. The axes are visible equipment; this change does not add combat controls.

`PlayerRoot` retains its original `ThirdPersonAvatarController`, CharacterController, input actions, and camera target. `Axe Warrior Avatar` uses the original animation clips and transitions through a dedicated `AxeWarriorAnimator` controller. Foot IK is enabled while idle, and the resting feet adapt to the terrain. Walking and running use their original animation poses so foot IK does not pull the knees sideways. Jumping and swimming retain their original animation poses.

See `AXE-WARRIOR-SETUP.md` for the current model configuration and `TENKOKU-SETUP.md` for the preserved sky timings.

