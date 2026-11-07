using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrystalGuardianOffering : MonoBehaviour
{
    public Transform player;
    public Transform crystal;
    public Transform offeringTarget;
    public InputActionReference interactAction;
    public Light eyeGlow;
    public AudioSource hum;
    public AudioSource guardianMelodyAudio;
    public TigerSequencePuzzle tigerPuzzle;
    [Min(0f)] public float tonePause = 0.25f;

    public float interactDistance = 3f;
    public float moveSpeed = 5f;

    public bool IsPlayerNearby => player &&
        Vector3.Distance(player.position, transform.position) <= interactDistance;
    public bool HasPlayedOpeningMelody { get; private set; }

    private bool offering;
    private bool completed;

    void Update()
    {
        if (!player || !interactAction) return;

        bool pressedNearby = interactAction.action.WasPressedThisFrame() && IsPlayerNearby;

        if (completed)
        {
            if (pressedNearby && tigerPuzzle && !tigerPuzzle.IsMelodyPlaying && !tigerPuzzle.IsComplete)
                StartCoroutine(PlayMelody(false));
            return;
        }

        if (!crystal || !offeringTarget) return;
        if (!offering && pressedNearby && crystal.parent != null && crystal.parent.name == "CrystalHandTarget")
        {
            offering = true;
            crystal.SetParent(null);
        }

        if (offering)
        {
            crystal.position = Vector3.MoveTowards(
                crystal.position,
                offeringTarget.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(crystal.position, offeringTarget.position) < 0.1f)
            {
                crystal.SetParent(offeringTarget);
                crystal.localPosition = Vector3.zero;
                offering = false;
                completed = true;
                if (eyeGlow) eyeGlow.enabled = true;
                if (hum) hum.Play();
                StartCoroutine(PlayMelody(true));
            }
        }
    }

    private IEnumerator PlayMelody(bool waitForHum)
    {
        if (!guardianMelodyAudio || !tigerPuzzle) yield break;
        tigerPuzzle.IsMelodyPlaying = true;

        if (waitForHum)
            yield return new WaitWhile(() => hum && hum.isPlaying);

        // Read the same order and clips that the tiles use.
        int playedTones = 0;
        foreach (int number in tigerPuzzle.order)
        {
            TigerTile tile = System.Array.Find(tigerPuzzle.tiles, t => t && t.tileNumber == number);
            if (!tile || !tile.tileSound || !tile.tileSound.clip) continue;
            if (playedTones > 0) yield return new WaitForSeconds(tonePause);

            guardianMelodyAudio.clip = tile.tileSound.clip;
            guardianMelodyAudio.Play();
            yield return new WaitWhile(() => guardianMelodyAudio.isPlaying);
            playedTones++;
        }

        tigerPuzzle.IsMelodyPlaying = false;
        if (waitForHum && playedTones > 0 && playedTones == tigerPuzzle.order.Length)
            HasPlayedOpeningMelody = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (!tigerPuzzle || !tigerPuzzle.IsMelodyPlaying) return;

        if (guardianMelodyAudio) guardianMelodyAudio.Stop();
        tigerPuzzle.IsMelodyPlaying = false;
    }
}
