# Armored axe warrior

Current scene: `Assets/Glowkeeper_Update.unity`.

The warrior uses the matching A1 outfit from the imported Axe Warrior package: chest plate, crossed straps/belt, shoulder plates, bracers, trousers, knee guards, and boots. Beige linen sleeves reuse the imported upper-arm skinning with a small cloth allowance to complete the shirt appearance in the supplied reference. Both native axes are visible. Bare leg/foot meshes are hidden underneath the clothes to avoid overlapping geometry. The imported FBX, prefab, and material assets remain unchanged.

The magic staff, StaffSocket, FireflyHoldPoint, the three collectible Firefly1Root objects, collection/deposit components, LampReceiver components, and their interaction-only light/destination helpers were removed. The decorative lamp meshes and the forest's ambient particle effects remain. The four unused collection/firefly-direction scripts were archived outside Assets. The other forest/recovery scene copies were cleaned of these script references as well.

The movement/camera source and the original WizardAnimator asset are unchanged. `Assets/GlowKeeper/Animation/AxeWarrior/AxeWarriorAnimator.controller` copies the original clips and state transitions while enabling foot IK for idle/walk/run and disabling it for jumping/swimming. Root motion remains disabled.

The visual model keeps its 0.9331225 scale. Its base is lowered to -0.065 in player-local Y to correct the floating boot offset. `WarriorFootPlacement` uses the calibrated boot-sole heights and terrain raycasts to place the resting feet and pelvis naturally on slopes, fading out when movement starts and switching off during jumping/swimming. The collision capsule, movement speeds, input bindings, and camera settings are retained.

Backups: `SetupBackup/WarriorArmor-2026-11-04`. The earlier model replacement backup remains at `SetupBackup/AxeWarrior-2026-11-04`.

Verification report: `AgentScripts/warrior-armor-playcheck.txt`. Final preview: `AgentScripts/warrior-armor-final.png`.

Final checks passed for full clothing and both axes, staff removal, left/right stance, walking/running states, jumping and swimming up/down. Fresh grounded-idle calibration measured boot clearances of about 2 mm (left) and 5 mm (right). All four affected scenes have no missing scripts; raw and final verification logs are retained under AgentScripts.

