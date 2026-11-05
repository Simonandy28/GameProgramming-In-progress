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

    public void TakeHit()
    {
        if (IsBroken || !detector.swarm.IsAtStone) return;
        Hits++;
        impactAudio.PlayOneShot(hitSound);
        if (Hits >= hitsToBreak) BreakStone();
        else if (Hits - 1 < crackStages.Length) crackStages[Hits - 1].SetActive(true);
    }

    private void BreakStone()
    {
        IsBroken = true;
        solidCollider.enabled = false;
        if (stoneLOD) stoneLOD.enabled = false;
        foreach (Renderer surface in stoneRenderers) surface.enabled = false;
        foreach (GameObject cracks in crackStages) cracks.SetActive(false);
        // Detection is finished. Leave the swarm and its target alive.
        detector.enabled = false;
        detector.blueGlow.SetActive(false);
        rubble.SetActive(true);
        sword.ReleaseFromStone();
        var pieces = rubble.GetComponentsInChildren<Rigidbody>();
        var playerCollider = detector.swarm.player.GetComponent<CharacterController>();
        foreach (Rigidbody piece in pieces)
        {
            var collider = piece.GetComponent<Collider>();
            Physics.IgnoreCollision(collider, playerCollider);
            Physics.IgnoreCollision(collider, sword.swordCollider);
            foreach (Rigidbody other in pieces)
                if (other != piece) Physics.IgnoreCollision(collider, other.GetComponent<Collider>());
            piece.isKinematic = false;
            Vector3 outward = (piece.position - transform.position).normalized;
            piece.AddForce((outward + Vector3.up * 0.5f) * 1.5f, ForceMode.Impulse);
        }
        impactAudio.PlayOneShot(breakSound);
    }
}
