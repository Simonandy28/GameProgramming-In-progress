using System.Collections;
using TMPro;
using UnityEngine;

public class FirstPuzzleMission : MonoBehaviour
{
    public FireflyStoneDetector detector;
    public PlayerSwordPuzzle player;

    public TextMeshProUGUI objective;
    public TextMeshProUGUI prompt;
    public TextMeshProUGUI subtitle;
    public CanvasGroup subtitleGroup;
    public AudioSource chimes;
    public AudioClip awakeningChime;
    public AudioClip victoryChime;
    [Range(0f, 1f)] public float objectiveBuzzVolume = 0.25f;

    private enum Stage { Approach, FindFireflies, FollowSwarm, BreakStone, ClaimSword, Complete }
    private Stage stage;
    private Coroutine subtitleRoutine;

    private void LateUpdate()
    {
        CheckProgress();
        UpdateObjectiveVisibility();
        UpdatePrompt();
    }

    // Read the existing puzzle's results. This script never handles input.
    private void CheckProgress()
    {
        if (stage == Stage.Approach && (IsLoud(detector.plantBuzz) || detector.swarm.HasLeftPlant))
        {
            stage = Stage.FindFireflies;
            objective.text = "Follow the mysterious humming.";
            if (!detector.swarm.HasLeftPlant)
                Say("Where whispers hum, forgotten steel lies.");
        }
        else if (stage == Stage.FindFireflies && detector.swarm.IsAtHand)
        {
            stage = Stage.FollowSwarm;
            objective.text = "Follow the bees' humming.";
            Say("Three wandering lights shall guide your way.",
                "Let their song unveil what stone conceals.");
        }
        else if (stage == Stage.FollowSwarm && detector.swarm.IsAtStone)
        {
            stage = Stage.BreakStone;
            objective.text = "Break the enchanted stone.";
            chimes.PlayOneShot(awakeningChime);
            Say("The sleeping stone has awakened.");
        }
        else if (stage == Stage.BreakStone && player.stone.IsBroken)
        {
            stage = Stage.ClaimSword;
            objective.text = "Press E to claim the sword.";
            Say("The forgotten blade awaits its keeper.");
        }
        else if (stage == Stage.ClaimSword && player.sword.IsHeld)
        {
            stage = Stage.Complete;
            chimes.PlayOneShot(victoryChime);
            Say("<color=#E8BD73>MISSION COMPLETED</color>", "The First Blade Acquired",
                "A blade is found. A legend begins.");
        }
    }

    private void UpdateObjectiveVisibility()
    {
        bool show = stage == Stage.BreakStone || stage == Stage.ClaimSword;
        if (stage == Stage.FindFireflies)
            show = !detector.swarm.HasLeftPlant && IsLoud(detector.plantBuzz);
        else if (stage == Stage.FollowSwarm)
            show = detector.swarm.IsAtHand && IsLoud(detector.buzzing);

        objective.transform.parent.gameObject.SetActive(show);
    }

    private bool IsLoud(AudioSource buzz)
    {
        return buzz && buzz.isPlaying && buzz.volume >= objectiveBuzzVolume;
    }

    private void UpdatePrompt()
    {
        string message = "";
        if (detector.swarm.CanAttract)
            message = "Press E to attract the fireflies.";
        else if (detector.CanAwakenStone)
            message = "Press E to awaken the stone.";
        else if (stage == Stage.BreakStone && !player.IsAttacking &&
            Vector3.Distance(player.transform.position, player.stone.transform.position) <= player.attackRange)
            message = "Face the stone and left click to strike.";
        else if (player.CanClaimSword)
            message = "Press E to claim the sword.";
        else if (player.sword.IsHeld)
            message = player.storedBladeCount < player.storedBlades.Length
                ? "Hold E again to store the sword." : "Blade storage is full.";

        prompt.text = message;
        prompt.transform.parent.gameObject.SetActive(message != "");
    }

    private void Say(params string[] lines)
    {
        if (subtitleRoutine != null) StopCoroutine(subtitleRoutine);
        subtitleRoutine = StartCoroutine(ShowSubtitles(lines));
    }

    // Time controls only the text fading. Gameplay decides the mission stage.
    private IEnumerator ShowSubtitles(string[] lines)
    {
        subtitleGroup.alpha = 0f;
        foreach (string line in lines)
        {
            subtitle.text = line;
            yield return FadeTo(1f);
            yield return new WaitForSeconds(3f);
            yield return FadeTo(0f);
        }
        subtitleRoutine = null;
    }

    private IEnumerator FadeTo(float alpha)
    {
        while (subtitleGroup.alpha != alpha)
        {
            subtitleGroup.alpha = Mathf.MoveTowards(subtitleGroup.alpha, alpha, 3f * Time.deltaTime);
            yield return null;
        }
    }
}
