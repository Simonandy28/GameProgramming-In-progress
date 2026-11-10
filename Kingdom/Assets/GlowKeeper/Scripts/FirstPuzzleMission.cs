using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FirstPuzzleMission : MonoBehaviour
{
    public FireflyStoneDetector detector;
    public PlayerSwordPuzzle player;

    [SerializeField] private TextMeshProUGUI objective;
    [SerializeField] private TextMeshProUGUI prompt;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private AudioSource chimes;
    [SerializeField] private AudioClip awakeningChime;
    [SerializeField] private AudioClip victoryChime;
    [Range(0f, 1f)] [SerializeField] private float objectiveBuzzVolume = 0.25f;

    private enum Stage { Approach, FindFireflies, FollowSwarm, BreakStone, ClaimSword, Complete }
    private Stage stage;
    private readonly Queue<(string text, float seconds)> subtitles = new Queue<(string, float)>();
    public bool IsShowingSubtitle { get; private set; }
    private RectTransform subtitlePanel;
    private float panelHeight;
    private float textHeight;
    private SecondPuzzleMission secondPuzzle;
    private RectTransform hintPanel;
    private Vector2 hintSize, hintPosition, hintTextSize;
    private TextWrappingModes hintWrapping;
    private bool wideHint;

    private void Awake()
    {
        subtitlePanel = (RectTransform)subtitleGroup.transform;
        panelHeight = subtitlePanel.rect.height;
        textHeight = subtitle.rectTransform.rect.height;
        subtitle.textWrappingMode = TextWrappingModes.Normal;
        secondPuzzle = GetComponent<SecondPuzzleMission>();
        if (secondPuzzle && prompt)
        {
            hintPanel = (RectTransform)prompt.transform.parent;
            hintSize = hintPanel.sizeDelta;
            hintPosition = hintPanel.anchoredPosition;
            hintTextSize = prompt.rectTransform.sizeDelta;
            hintWrapping = prompt.textWrappingMode;
        }
    }

    private void LateUpdate()
    {
        CheckProgress();
        UpdateObjectiveVisibility();
        UpdatePrompt();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        subtitles.Clear();
        IsShowingSubtitle = false;
        if (subtitleGroup) subtitleGroup.alpha = 0f;
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
        bool puzzleTwoHint = false;
        if (detector.swarm.CanAttract)
            message = "Press E to attract the fireflies.";
        else if (detector.CanAwakenStone)
            message = "Press E to awaken the stone.";
        else if (stage == Stage.BreakStone && !player.IsAttacking &&
            Vector3.Distance(player.transform.position, player.stone.transform.position) <= player.attackRange)
            message = "Face the stone and left click to strike.";
        else if (secondPuzzle && secondPuzzle.isActiveAndEnabled && secondPuzzle.TryGetInteractionHint(out message))
            puzzleTwoHint = true;
        else if (player.CanClaimSword)
            message = player.CanClaimCursedBlade
                ? "Press E to claim the Cursed Blade." : "Press E to claim the sword.";
        else if (player.HeldSword)
            message =  "Hold E again to store the sword.";

        // Give the longer crystal hints two lines, then restore the normal prompt layout.
        if (hintPanel && wideHint != puzzleTwoHint)
        {
            wideHint = puzzleTwoHint;
            hintPanel.sizeDelta = wideHint ? new Vector2(760f, 68f) : hintSize;
            hintPanel.anchoredPosition = wideHint ? hintPosition + Vector2.down * 25f : hintPosition;
            prompt.rectTransform.sizeDelta = wideHint ? new Vector2(706f, 60f) : hintTextSize;
            prompt.textWrappingMode = wideHint ? TextWrappingModes.Normal : hintWrapping;
        }
        prompt.text = message;
        prompt.transform.parent.gameObject.SetActive(message != "");
    }

    private void Say(params string[] lines)
    {
        foreach (string line in lines) subtitles.Enqueue((line, 3f));
        if (!IsShowingSubtitle) StartCoroutine(ShowSubtitles());
    }

    public void SayQuestHint(string line)
    {
        subtitles.Enqueue((line, 5f));
        if (!IsShowingSubtitle) StartCoroutine(ShowSubtitles());
    }

    // Time controls only the text fading. Gameplay decides the mission stage.
    private IEnumerator ShowSubtitles()
    {
        IsShowingSubtitle = true;
        subtitleGroup.alpha = 0f;
        while (subtitles.Count > 0)
        {
            var (text, seconds) = subtitles.Dequeue();
            subtitle.text = text;
            float extraHeight = Mathf.Max(0f, subtitle.preferredHeight + 8f - textHeight);
            subtitle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight + extraHeight);
            subtitlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight + extraHeight);
            yield return FadeTo(1f);
            yield return new WaitForSeconds(seconds);
            yield return FadeTo(0f);
        }
        IsShowingSubtitle = false;
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
