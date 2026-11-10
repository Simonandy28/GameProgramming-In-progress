
using UnityEngine;

public class Puzzle3Guardian : MonoBehaviour
{
    [SerializeField] private Puzzle3Crystal[] crystals;
    [SerializeField] private Puzzle3Ritual ritual;
    [SerializeField] private Light guardianLight;
    [SerializeField] private AudioSource hum;
    [SerializeField] private GameObject platforms;
    [SerializeField] private FirstPuzzleMission guidance;
    [SerializeField] private PlayerSwordPuzzle player;
    [SerializeField] private Puzzle3PoisonWater poison;
    [SerializeField] private Collider firstPillar, lastPillar;
    [SerializeField] private Vector3 exitShore;

    private bool activated;
    private bool sawWater, sawPath, sawDayPlate, sawNightPlate, carriedCrystal;
    private bool sawBridge, sawBlade, claimedBlade, completed;
    private int offeredCount, exitPillarRespawn = -1;
    private CharacterController walker;
    private BoxCollider water;

    private void Start()
    {
        if (guardianLight) guardianLight.enabled = false;
        if (platforms) platforms.SetActive(false);
        if (player) walker = player.GetComponent<CharacterController>();
        if (poison) water = poison.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (activated || !ritual || !ritual.IsActive ||
            crystals == null || crystals.Length != 3) return;

        foreach (Puzzle3Crystal crystal in crystals)
            if (!crystal || !crystal.Offered) return;

        activated = true;
        if (guardianLight) guardianLight.enabled = true;
        if (hum) hum.Play();
        if (platforms) platforms.SetActive(true);
        if (guidance) guidance.SayQuestHint("The three lights unite, and the ancient guardian awakens. Beyond the altar, a path rises from the poisoned depths.");
        Debug.Log("Puzzle 3 Guardian Activated!");
    }

    private void LateUpdate()
    {
        if (completed || !guidance || !walker || !water || !ritual || crystals == null) return;

        Vector3 position = walker.transform.position;
        Vector3 atWater = new Vector3(position.x, water.bounds.max.y, position.z);
        if (position.y < atWater.y + 6f && Vector3.Distance(atWater, water.ClosestPoint(atWater)) < 8f)
            SayOnce(ref sawWater, "Beware the cursed waters, where even the bravest fall. One touch of the poisoned depths shall seal your fate.");

        if (!activated)
        {
            if (sawWater && Vector3.Distance(position, ritual.transform.position) < 14f)
                SayOnce(ref sawPath, "Above these cursed depths, an ancient guardian keeps its watch. Climb the stones and seek the forgotten altar.");
            if (ritual.PlayerOnPlate && !ritual.IsActive)
                SayOnce(ref sawDayPlate, "The spirits slumber beneath the sun. Return when the moon rises, and three hidden lights shall answer your call.");
            if (ritual.IsActive)
                SayOnce(ref sawNightPlate, "Three wandering lights awaken beneath the moon. Seek their glow among the rocks, and return them to the guardian's stone altars.");

            int offers = 0;
            foreach (Puzzle3Crystal crystal in crystals)
            {
                if (!crystal) continue;
                if (crystal.IsCarried)
                    SayOnce(ref carriedCrystal, "The ancient light rests in your hands. Carry it to its sacred tablet and return what was lost.");
                if (crystal.Offered) offers++;
            }
            if (offers > offeredCount && offers == 1)
                guidance.SayQuestHint("One sacred light returns. The guardian stirs, awaiting the remaining offerings.");
            if (offers > offeredCount && offers == 2)
                guidance.SayQuestHint("Two lights have awakened. One final offering shall summon the guardian.");
            offeredCount = offers;
            return;
        }

        if (firstPillar && Vector3.Distance(position, firstPillar.bounds.ClosestPoint(position)) < 5f)
            SayOnce(ref sawBridge, "Stone rises, stone falls, and death waits beneath. Time each leap, brave warrior, and cross the cursed waters to claim your blade.");
        if (player.CanClaimCursedBlade)
            SayOnce(ref sawBlade, "Upon the highest stone lies the blade of the forgotten king. Press E to claim its power.");
        if (player.IsHoldingCursedBlade)
            SayOnce(ref claimedBlade, "The Cursed Blade is yours! Yet your trial is not over. Return across the rising stones and reach the safety of the shore.");
        if (!claimedBlade || !walker.isGrounded) return;

        // A fall invalidates the last pillar visit. Reach it again before the exit counts.
        if (lastPillar && position.y >= atWater.y &&
            Physics.Raycast(position + Vector3.up * 0.15f, Vector3.down,
                out RaycastHit ground, 0.55f, ~0, QueryTriggerInteraction.Ignore) &&
            ground.collider == lastPillar)
            exitPillarRespawn = poison.RespawnCount;

        if (exitPillarRespawn == poison.RespawnCount && Vector3.Distance(position, exitShore) < 3f)
            SayOnce(ref completed, "Trial Complete! The cursed waters have been conquered, and the ancient blade now belongs to you.");
    }

    private void SayOnce(ref bool shown, string message)
    {
        if (shown) return;
        shown = true;
        guidance.SayQuestHint(message);
    }
}
