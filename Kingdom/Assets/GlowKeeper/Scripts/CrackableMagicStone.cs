using UnityEngine;

public class CrackableMagicStone : MonoBehaviour
{
    public FireflyStoneDetector detector;
    public FloatingSword sword;
    public Collider solidCollider;
    public LODGroup stoneLOD;
    public Renderer[] stoneRenderers;
    public GameObject[] crackStages;
    public GameObject rubble;
    public AudioSource impactAudio;
    public AudioClip hitSound;
    public AudioClip breakSound;
    public int hitsToBreak = 3;
    public bool IsBroken { get; private set; }
    public int Hits { get; private set; }

    public void TakeHit(CharacterController playerCollider = null)
    {
        // A test scene can use the stone without setting up the fireflies.
        if (IsBroken || (detector && (!detector.swarm || !detector.swarm.IsAtStone))) return;
        Hits++;
        impactAudio.PlayOneShot(hitSound);
        if (Hits >= hitsToBreak) BreakStone(playerCollider);
        else if (Hits - 1 < crackStages.Length) crackStages[Hits - 1].SetActive(true);
    }

    private void BreakStone(CharacterController playerCollider)
    {
        IsBroken = true;
        solidCollider.enabled = false;
        if (stoneLOD) stoneLOD.enabled = false;
        foreach (Renderer surface in stoneRenderers) surface.enabled = false;
        foreach (GameObject cracks in crackStages) cracks.SetActive(false);
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
