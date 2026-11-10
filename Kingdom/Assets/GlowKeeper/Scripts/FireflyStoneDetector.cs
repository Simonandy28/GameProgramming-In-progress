
using UnityEngine;
using UnityEngine.UI;

public class FireflyStoneDetector : MonoBehaviour
{
    [Header("Drag in the scene objects")]
    public FireflyHandAttraction swarm;
    public Transform magicStone;
    public Transform stoneBeeTarget;
    public AudioSource buzzing;
    public Slider detectorMeter;
    public GameObject blueGlow;
    [SerializeField] private Light statueEyeGlow;
    [SerializeField] private AudioSource statueHum;
    [Header("Buzzing at the plant")]
    public AudioSource plantBuzz;
    [Min(0.1f)] [SerializeField] private float plantBuzzRange = 14f;
    [Range(0f, 1f)] [SerializeField] private float plantMaxVolume = 0.45f;

    [Header("Detector settings")]
    [Min(0.1f)] public float detectionRange = 65f;
    [Min(0.1f)] public float closeActivationDistance = 3.5f;
    [Min(0.1f)] [SerializeField] private float signalSmoothing = 3f;
    [Range(0f, 1f)] [SerializeField] private float minVolume = 0.06f;
    [Range(0f, 1f)] [SerializeField] private float maxVolume = 0.6f;
    [Range(0.1f, 3f)] [SerializeField] private float minPitch = 0.85f;
    [Range(0.1f, 3f)] [SerializeField] private float maxPitch = 1.3f;

    // Changed: Check distance to the statue instead of the stone.
    public bool CanAwakenStone => swarm.IsAtHand && Vector3.Distance(swarm.player.position, stoneBeeTarget.position) <= closeActivationDistance;

    private float strength;
    private float StoneDistance => Vector3.Distance(swarm.player.position, magicStone.position);

    private void Update()
    {
        if (!swarm || !magicStone || !stoneBeeTarget) return;

        UpdatePlantBuzz();

        if (CanAwakenStone && swarm.interactAction.action.WasPressedThisFrame())
            swarm.FlyToStone(stoneBeeTarget);

        // The detector works only while the bees are at the player's hand.
        bool searching = swarm.IsAtHand;
        if (detectorMeter) detectorMeter.gameObject.SetActive(searching);
        if (blueGlow && swarm.IsAtStone) blueGlow.SetActive(true);
        if (statueEyeGlow) statueEyeGlow.enabled = swarm.IsAtStone;
        if (statueHum && swarm.IsAtStone && !statueHum.isPlaying) statueHum.Play();
        if (!searching)
        {
            strength = 0f;
            if (buzzing && buzzing.isPlaying) buzzing.Stop();
            return;
        }

        Vector3 direction = magicStone.position - swarm.player.position;
        direction.y = 0f; // Look left/right, not up/down at the ground.
        float facing = direction.sqrMagnitude < 0.001f ? 1f :
            Mathf.Clamp01(Vector3.Dot(swarm.player.forward, direction.normalized));
        float proximity = 1f - Mathf.Clamp01(StoneDistance / detectionRange);
        float wantedStrength = facing * proximity;

        // Smooth one signal, then use it for both sound and the bar.
        strength = Mathf.Lerp(strength, wantedStrength, 1f - Mathf.Exp(-signalSmoothing * Time.deltaTime));
        if (detectorMeter) detectorMeter.value = strength;
        if (buzzing)
        {
            buzzing.volume = Mathf.Lerp(minVolume, maxVolume, strength);
            buzzing.pitch = Mathf.Lerp(minPitch, maxPitch, strength);
            if (!buzzing.isPlaying) buzzing.Play();
        }
    }

    private void UpdatePlantBuzz()
    {
        if (!plantBuzz || !swarm.player) return;

        float plantDistance = Vector3.Distance(swarm.player.position, plantBuzz.transform.position);
        float closeness = 1f - Mathf.Clamp01(plantDistance / plantBuzzRange);
        float wantedVolume = swarm.HasLeftPlant ? 0f : closeness * plantMaxVolume;
        plantBuzz.volume = Mathf.Lerp(plantBuzz.volume, wantedVolume, 1f - Mathf.Exp(-4f * Time.deltaTime));
        if (plantBuzz.volume > 0.001f)
        {
            if (!plantBuzz.isPlaying) plantBuzz.Play();
        }
        else if (plantBuzz.isPlaying) plantBuzz.Stop();
    }

    private void OnDisable()
    {
        if (buzzing) buzzing.Stop();
        if (plantBuzz) plantBuzz.Stop();
        if (detectorMeter) detectorMeter.gameObject.SetActive(false);
    }
}
