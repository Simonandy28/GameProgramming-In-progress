# Firefly hand attraction

The scene `Assets/Glowkeeper_Update.unity` is already connected.

## Test

1. Press Play and walk toward the firefly plant.
2. Within 3 metres of the `BeeSwarm` anchor, the screen shows
   **Press E to attract fireflies**.
3. Tap **E**. The supplied **Standing 1H Magic Attack 01** animation plays once,
   and the three fireflies travel toward `BeeHandTarget`.
4. Walk, run and turn. They keep hovering around the moving right hand.
5. The detector now guides you to `MagicStone`. Press E within 3.5 metres of
   that stone to send the swarm there. Stop and restart Play to reset the test.

## One simple script

`FireflyHandAttraction.cs` is attached to **BeeSwarm** only. It does not generate
figure-eight movement. Your three existing Animator flight clips keep running.

On E, the script remembers the gap between the swarm anchor and the hand.
Every LateUpdate it closes that gap using `movementSpeed * Time.deltaTime`.
Once the gap reaches zero, the anchor follows the animated hand exactly.
It stays under its existing parent, so the hand's scale cannot resize the insects.

After the flight Animators update, the script makes their local flight positions
smaller to form a compact hand hover. It preserves animation time, phase,
individual flight speed, and every model's scale. Keep all three flight Animators
enabled with **Culling Mode = Always Animate** and **Update Mode = Normal**.

The prompt uses the built-in `OnGUI` system, so no Canvas or UI package setup is
needed. The existing Input System asset has a new **Player/Interact** action
bound to **E**, with no Hold interaction.

## Inspector fields on BeeSwarm

| Field | Assigned object or asset |
| --- | --- |
| Player | `PlayerRoot` |
| Bee Hand Target | `PlayerRoot > Axe Warrior Avatar > ... > Hand.R > BeeHandTarget` |
| Player Animator | Animator on `Axe Warrior Avatar` |
| Interact Action | `Assets/GlowKeeper/Input/PlayerControls.inputactions` > `Player/Interact` |
| Fireflies, Size 3 | The `Firefly_Fred` model inside each of `Firefly_1`, `Firefly_2`, `Firefly_3` |
| Interaction Distance | 3 metres, measured from PlayerRoot to BeeSwarm |
| Movement Speed | 5 metres per second of gap closure |
| Hand Formation Size | 0.3, giving 30% of the original path size; model size stays unchanged |

To assign **Interact Action** manually, use its object-picker circle and select
`PlayerControls: Player/Interact`. Assign the **Fred models** in the array,
not their Animator holders.

## Player animation

The supplied FBX is imported as Humanoid at
`Assets/GlowKeeper/Animation/AxeWarrior/Standing 1H Magic Attack 01.fbx`.
Its **Attract Fireflies** clip is a non-looping, 2.3-second gesture.

The existing `AxeWarriorAnimator.controller` has an added **Firefly Casting**
layer and **AttractFireflies** trigger:

`Ready -> Attract Fireflies -> Ready`

An upper-body Avatar Mask allows the existing lower-body locomotion to continue.
The base animation layer and player movement script are unchanged.
The stone detector, buzzing and blue activation effect are documented in
`Assets/GlowKeeper/Puzzle/README.md`. Both scripts are attached to BeeSwarm.
