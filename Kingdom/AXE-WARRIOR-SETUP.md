# Armored axe warrior

Current scene: `Assets/Glowkeeper_Update.unity`.

The warrior uses the matching A1 outfit from the imported Axe Warrior package: chest plate, crossed straps/belt, shoulder plates, bracers, trousers, knee guards, and boots. Beige linen sleeves reuse the imported upper-arm skinning with a small cloth allowance to complete the shirt appearance in the supplied reference. Both native axes are visible. Bare leg/foot meshes are hidden underneath the clothes to avoid overlapping geometry. The imported FBX, prefab, and material assets remain unchanged.

The magic staff, StaffSocket, FireflyHoldPoint, the three collectible Firefly1Root objects, collection/deposit components, LampReceiver components, and their interaction-only light/destination helpers were removed. The decorative lamp meshes and the forest's ambient particle effects remain. The four unused collection/firefly-direction scripts were archived outside Assets. The other forest/recovery scene copies were cleaned of these script references as well.

The movement/camera source and the original WizardAnimator asset are unchanged. `Assets/GlowKeeper/Animation/AxeWarrior/AxeWarriorAnimator.controller` copies the original clips and state transitions while enabling foot IK for idle and disabling it for walking, running, jumping and swimming. Root motion remains disabled.

The visual model keeps its 0.9331225 scale. Its base is lowered to -0.065 in player-local Y to correct the floating boot offset. `WarriorFootPlacement` uses the calibrated boot-sole heights and terrain raycasts to place the resting feet and pelvis naturally on slopes, releasing both feet immediately when movement starts, easing out the remaining pelvis-height adjustment, and switching off during jumping/swimming. The collision capsule, movement speeds, input bindings, and camera settings are retained.

Backups: `SetupBackup/WarriorArmor-2026-11-04`. The earlier model replacement backup remains at `SetupBackup/AxeWarrior-2026-11-04`.

Verification report: `AgentScripts/warrior-armor-playcheck.txt`. Final preview: `AgentScripts/warrior-armor-final.png`.

Final checks passed for full clothing and both axes, staff removal, left/right stance, walking/running states, jumping and swimming up/down. Fresh grounded-idle calibration measured boot clearances of about 2 mm (left) and 5 mm (right). All four affected scenes have no missing scripts; raw and final verification logs are retained under AgentScripts.


## Walking/running leg correction

Locomotion foot IK was adding sideways knee motion to the retargeted clips. It is now disabled for Walking and Running; the clips, state transitions and movement controls are unchanged. Resting terrain foot placement releases its foot position/rotation constraints before the first stride, while its remaining body-height offset eases out. It only re-engages after the Animator has fully reached Idle and completed its transition, so it cannot pull a leg during a walk/run blend. Idle ground contact is retained.

The deterministic 180-sample walk audit reduced the largest left-knee bending-plane change from about 45.1 to 5.5 degrees. The running audit reduced maximum lateral knee deviation from about 0.094 to 0.060 metres. Both retain their natural knee-bending ranges. Reports: AgentScripts/warrior-leg-fix-audit.txt, AgentScripts/warrior-leg-fix-live-motion.txt, and AgentScripts/warrior-leg-fix-playcheck.txt. Pre-change assets and scene are under SetupBackup/WarriorLegFix-2026-11-04.


