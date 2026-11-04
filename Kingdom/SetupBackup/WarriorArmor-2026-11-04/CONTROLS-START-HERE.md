# Current forest controls

Open `Assets/Glowkeeper_Update.unity` and press Play. This is the scene with the current animated avatar, houses, statues, and five swimming volumes, and it is the enabled build scene.

- W/A/S/D: move relative to the camera.
- Left Shift: run.
- Space: jump on land; ascend while swimming.
- Left Ctrl: descend while swimming.
- Mouse: orbit the third-person camera.
- Escape: release the cursor; click the Game view to capture it again.
- X: collect nearby fireflies with the staff.
- E: deposit collected fireflies into a nearby unlit lamp.

`PlayerRoot` retains its `ThirdPersonAvatarController`, CharacterController, input actions, and `Axe Warrior Avatar` Animator (retargeted to the original animation controller). `Main Camera` retains `ThirdPersonCamera`. The five WaterVolume triggers and the avatar's walk, run, jump, idle, and swimming animations are preserved.

The zombie AI, zombie model and animations, zombie navigation helpers, player damage/death/game-over scripts, and obsolete disabled day/night controller were removed. There is no health/game-over freeze. See `WORLD-BASELINE.md` for cleanup and verification details and `TENKOKU-SETUP.md` for the preserved sky timings.

