# Axe warrior avatar

The active `Assets/Glowkeeper_Update.unity` scene now uses `Assets/Axe Warrior/Prefab/Axe_Warrior.prefab` as `PlayerRoot/Axe Warrior Avatar`.

Only the old humanoid figure was replaced. The player root, collision capsule, input actions, movement settings, third-person camera, water triggers, world, and day/night schedule were retained. No gameplay script source or animation-controller asset was changed. The warrior uses its own valid Humanoid Avatar to retarget the existing idle, walking, running, jumping, and swimming animations.

The warrior scale is 0.9331225, matching the former figure's approximately 1.815-metre visual height. Its feet are aligned just above the existing collision capsule base. Root motion remains disabled as before; movement is driven by the original CharacterController code.

The existing staff, ParentConstraint, StaffSocket, and FireflyHoldPoint were transferred to the warrior's right hand, with a grip-frame adjustment for the different bone axes. The warrior retains its native left-hand axe. The right-hand axe is hidden on this scene instance so it does not overlap the functional staff. The source warrior prefab and imported materials were not edited.

Play-mode checks passed for humanoid retargeting, walking, running, jumping, entering water, reaching the actual Swimming Animator state, swimming up/down, collecting all three fireflies with X, following the staff hold point, depositing them with E, lighting the lamp, and the camera target. The saved scene has zero missing scripts. Runtime test movements and lamp/firefly changes were discarded when Play stopped.

The pre-swap scene is backed up outside Assets in `SetupBackup/AxeWarrior-2026-11-04`. The old model assets remain available because their animation assets are still used; the old figure is removed from the active scene.

Reports and previews: `AgentScripts/axe-warrior-replacement.txt`, `AgentScripts/axe-warrior-playcheck.txt`, `AgentScripts/axe-warrior-final-check.txt`, and `AgentScripts/axe-warrior-preview.png`.
