using UnityEngine;

// Watch Puzzle 2 progress and send each clue to the existing subtitle panel.
public class SecondPuzzleMission : MonoBehaviour
{
    public FirstPuzzleMission guidance;
    public UnderwaterCrystalAttraction crystal;
    public CrystalGuardianOffering guardian;
    public CrackableMagicStone rewardStone;
    [Min(0f)] public float discoveryDistance = 24f;

    private Vector3 crystalStart;
    private bool sawWater, tookCrystal, reachedGuardian, heardMelody, sawGlow, sawSword;
    private int wrongTileCount;
    private bool wrongHintPending;

    private void Awake()
    {
        if (crystal) crystalStart = crystal.transform.position;
    }

    private void LateUpdate()
    {
        if (!guidance || !crystal || !crystal.player || !guardian || !guardian.tigerPuzzle || !rewardStone) return;

        bool carrying = crystal.handTarget && crystal.transform.parent == crystal.handTarget;
        var tiger = guardian.tigerPuzzle;

        if (!sawWater && !tookCrystal && Vector3.Distance(crystal.player.position, crystalStart) <= discoveryDistance)
        {
            sawWater = true;
            guidance.SayQuestHint("Beneath the waters, a forgotten light lies sleeping. Seek the crystal where silence reigns.");
        }
        if (!tookCrystal && carrying)
        {
            tookCrystal = true;
            guidance.SayQuestHint("The light has chosen its bearer. Rise from the depths and seek the ancient king of the forest.");
        }
        if (!reachedGuardian && carrying && guardian.IsPlayerNearby)
        {
            reachedGuardian = true;
            guidance.SayQuestHint("Before the forest king, return the stolen light. Awaken his spirit, and hear the song of stone.");
        }
        if (!heardMelody && guardian.HasPlayedOpeningMelody && !tiger.IsComplete)
        {
            heardMelody = true;
            guidance.SayQuestHint("Four beasts guard the ancient way. Follow their voices in the order the king has sung.");
        }

        if (wrongTileCount != tiger.WrongTileCount)
        {
            wrongTileCount = tiger.WrongTileCount;
            wrongHintPending = guardian.HasPlayedOpeningMelody;
        }
        // Let a current clue finish and drop the reminder if the player has already solved the tiles.
        if (wrongHintPending && !tiger.IsComplete && !tiger.IsMelodyPlaying && !guidance.IsShowingSubtitle)
        {
            wrongHintPending = false;
            guidance.SayQuestHint("The forest remembers every false note. Return to the king, and listen once more.");
        }
        if (!sawGlow && tiger.IsComplete && rewardStone.rewardGlow && rewardStone.rewardGlow.activeInHierarchy)
        {
            sawGlow = true;
            guidance.SayQuestHint("The four voices now sing as one. The sleeping stone awakens. Shatter its seal and claim your reward.");
        }
        if (!sawSword && rewardStone.IsBroken && rewardStone.sword && rewardStone.sword.IsHovering)
        {
            sawSword = true;
            guidance.SayQuestHint("From ancient stone, a blade is born. Take it, bearer of the forgotten light.");
        }
    }
}
