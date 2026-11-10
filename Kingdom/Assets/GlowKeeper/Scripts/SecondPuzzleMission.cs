using UnityEngine;

// Watch Puzzle 2 progress and send each clue to the existing subtitle panel.
public class SecondPuzzleMission : MonoBehaviour
{
    public FirstPuzzleMission guidance;
    public UnderwaterCrystalAttraction crystal;
    public CrystalGuardianOffering guardian;
    public CrackableMagicStone rewardStone;
    // Kept for existing copies. The cove area now controls discovery.
    [Min(0f), HideInInspector] public float discoveryDistance = 24f;
    [SerializeField] private Rect crystalLakeArea;

    private ThirdPersonAvatarController swimmer;
    private bool sawWater, tookCrystal, heardMelody, sawGlow, sawSword;
    private int wrongTileCount;
    private bool wrongHintPending;

    private void Awake()
    {
        if (crystal && crystal.player)
            swimmer = crystal.player.GetComponent<ThirdPersonAvatarController>();
    }

    public bool TryGetInteractionHint(out string message)
    {
        message = "";
        if (crystal && crystal.CanCollect)
            message = "An ancient crystal shines before you. Press E to claim its light.";
        else if (guardian && guardian.CanOffer)
            message = "The Lion Guardian awaits its offering. Press E to surrender the crystal and awaken the song of stone.";
        return message != "";
    }

    private void LateUpdate()
    {
        if (!guidance || !crystal || !crystal.player || !guardian || !guardian.tigerPuzzle || !rewardStone) return;

        bool carrying = crystal.handTarget && crystal.transform.parent == crystal.handTarget;
        var tiger = guardian.tigerPuzzle;

        Vector3 position = crystal.player.position;
        if (!sawWater && !tookCrystal && swimmer && swimmer.IsSwimming &&
            crystalLakeArea.Contains(new Vector2(position.x, position.z)))
        {
            sawWater = true;
            guidance.SayQuestHint("Beneath these waters, an ancient light lies hidden. Seek the crystal in the depths, for its power shall lead you to a forgotten blade.");
        }
        if (!tookCrystal && carrying)
        {
            tookCrystal = true;
            guidance.SayQuestHint("The light is yours. Seek the ancient Lion Guardian upon the rocks, and offer this crystal to awaken its forgotten power.");
        }
        if (!heardMelody && guardian.HasPlayedOpeningMelody && !tiger.IsComplete)
        {
            heardMelody = true;
            guidance.SayQuestHint("Four beasts guard the ancient way. Remember the guardian's song, and follow their voices in the order heard.");
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
            guidance.SayQuestHint("The song has been broken. Return to the Lion Guardian and listen once more.");
        }
        if (!sawGlow && tiger.IsComplete && rewardStone.rewardGlow && rewardStone.rewardGlow.activeInHierarchy)
        {
            sawGlow = true;
            guidance.SayQuestHint("The four voices sing as one. The ancient stone awakens. Break its seal and claim the blade within.");
        }
        if (!sawSword && rewardStone.IsBroken && rewardStone.sword && rewardStone.sword.IsHovering)
        {
            sawSword = true;
            guidance.SayQuestHint("From ancient stone, a blade is born. Take it, bearer of the forgotten light.");
        }
    }
}
