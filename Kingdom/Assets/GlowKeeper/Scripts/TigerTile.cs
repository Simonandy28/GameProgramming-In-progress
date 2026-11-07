using System.Collections.Generic;
using UnityEngine;

public class TigerTile : MonoBehaviour
{
    public Light blueLight;
    public AudioSource tileSound;
    public TigerSequencePuzzle puzzle;
    public int tileNumber;

    private readonly HashSet<Collider> playerContacts = new HashSet<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !playerContacts.Add(other) || playerContacts.Count != 1)
            return;

        // Count the entry, but stay quiet until the guardian finishes.
        if (puzzle && puzzle.IsMelodyPlaying) return;

        if (tileSound)
            tileSound.Play();
        if (puzzle)
            puzzle.CheckTile(tileNumber);
    }

    void OnTriggerExit(Collider other)
    {
        playerContacts.Remove(other);
    }

    public void Activate()
    {
        if (blueLight) blueLight.enabled = true;
    }

    public void ResetTile()
    {
        if (blueLight) blueLight.enabled = false;
    }
}
