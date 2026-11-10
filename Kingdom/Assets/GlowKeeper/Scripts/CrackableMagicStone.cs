using UnityEngine;

public class CrackableMagicStone : MonoBehaviour
{
    public FireflyStoneDetector detector;
    public TigerSequencePuzzle tigerPuzzle;
    public GameObject rewardGlow;
    public FloatingSword sword;
    public Collider solidCollider;
    [SerializeField] private LODGroup stoneLOD;
    [SerializeField] private Renderer[] stoneRenderers;
    [SerializeField] private GameObject rubble;
    [SerializeField] private AudioSource impactAudio;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;
    public int hitsToBreak = 3;
    public bool IsBroken { get; private set; }
    public int Hits { get; private set; }
    private bool CanBeHit => !IsBroken &&
        (!detector || (detector.swarm && detector.swarm.IsAtStone)) &&
        (!tigerPuzzle || tigerPuzzle.IsComplete);

    private void Update()
    {
        if (tigerPuzzle && CanBeHit && rewardGlow && !rewardGlow.activeSelf)
            rewardGlow.SetActive(true);
    }

    public void TakeHit(CharacterController playerCollider = null)
    {
        if (!CanBeHit) return;
        Hits++;
        impactAudio.PlayOneShot(hitSound);
        if (Hits >= hitsToBreak) BreakStone(playerCollider);
    }

    private void BreakStone(CharacterController playerCollider)
    {
        IsBroken = true;
        solidCollider.enabled = false;
        if (stoneLOD) stoneLOD.enabled = false;
        foreach (Renderer surface in stoneRenderers) surface.enabled = false;
        if (rewardGlow) rewardGlow.SetActive(false);
        // Detection is finished. Leave the swarm and its target alive.
        if (detector)
        {
            detector.enabled = false;
            if (detector.blueGlow) detector.blueGlow.SetActive(false);
        }
        rubble.SetActive(true);
        if (sword) sword.ReleaseFromStone();
        var pieces = rubble.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody piece in pieces)
        {
            var collider = piece.GetComponent<Collider>();
            if (playerCollider) Physics.IgnoreCollision(collider, playerCollider);
            if (sword) Physics.IgnoreCollision(collider, sword.swordCollider);
            foreach (Rigidbody other in pieces)
                if (other != piece) Physics.IgnoreCollision(collider, other.GetComponent<Collider>());
            piece.isKinematic = false;
            Vector3 outward = (piece.position - transform.position).normalized;
            piece.AddForce((outward + Vector3.up * 0.5f) * 1.5f, ForceMode.Impulse);
        }
        impactAudio.PlayOneShot(breakSound);
    }
}
