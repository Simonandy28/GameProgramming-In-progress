using UnityEngine;

public class MagicStoneGlow : MonoBehaviour
{
    public Renderer stoneSurface;
    public Light blueLight;
    [Min(0.1f)] public float fadeDuration = 3f;
    [ColorUsage(false, true)] public Color glowColor = new Color(0f, 0.65f, 3f);
    [Min(0f)] public float lightBrightness = 9f;

    private Material glowMaterial;
    private float brightness;

    private void OnEnable()
    {
        // This object is enabled only when the bees reach the stone.
        if (!glowMaterial && stoneSurface)
        {
            glowMaterial = stoneSurface.material;
            glowMaterial.EnableKeyword("_EMISSION");
        }
        brightness = 0f;
        SetGlow();
    }

    private void Update()
    {
        // Take a few seconds to go from no glow to full magical blue.
        brightness = Mathf.MoveTowards(brightness, 1f, Time.deltaTime / fadeDuration);
        SetGlow();
    }

    private void SetGlow()
    {
        if (glowMaterial)
        {
            glowMaterial.SetColor("_BaseColor", new Color(0.02f, 0.25f, 1f, brightness * 0.8f));
            glowMaterial.SetColor("_EmissionColor", glowColor * brightness);
        }
        if (blueLight) blueLight.intensity = lightBrightness * brightness;
    }

    private void OnDestroy()
    {
        if (glowMaterial) Destroy(glowMaterial);
    }
}
