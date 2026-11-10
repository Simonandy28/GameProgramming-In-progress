using System.Collections.Generic;
using UnityEngine;

public class TigerTile : MonoBehaviour
{
    [SerializeField] private Light blueLight;
    public AudioSource tileSound;
    public TigerSequencePuzzle puzzle;
    public int tileNumber;

    private readonly HashSet<Collider> playerContacts = new HashSet<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !playerContacts.Add(other) || playerContacts.Count != 1)
            return;

        // Remember the entry so the player must step off and back on after the song.
        if (!puzzle || !puzzle.CanUseTiles) return;

        if (tileSound)
            tileSound.Play();
        puzzle.CheckTile(tileNumber);
    }

    void OnTriggerExit(Collider other)
    {
        playerContacts.Remove(other);
    }

    public void Activate()
    {
        if (puzzle && puzzle.CanUseTiles && blueLight) blueLight.enabled = true;
    }

    public void ResetTile()
    {
        if (blueLight) blueLight.enabled = false;
    }
}
