using UnityEngine;
using UnityEngine.UI;

public class SimpleVignetteEffect : MonoBehaviour
{
    [Header("Vignette Settings")]
    public Image vignetteImage;  // Assign a UI Image with a radial gradient
    [Range(0, 1)]
    public float defaultIntensity = 0.5f;
    [Range(0, 1)]
    public float maxIntensity = 0.9f;
    public float transitionSpeed = 2.0f;

    private float targetIntensity;
    private float currentIntensity;

    void Start()
    {
        if (vignetteImage == null)
        {
            Debug.LogError("Vignette image not assigned! Add a UI Image with a radial gradient texture.");
            enabled = false;
            return;
        }

        currentIntensity = defaultIntensity;
        targetIntensity = defaultIntensity;

        // Set initial alpha
        Color color = vignetteImage.color;
        color.a = currentIntensity;
        vignetteImage.color = color;
    }

    void Update()
    {
        // Smoothly transition intensity
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * transitionSpeed);

        // Update vignette alpha
        Color color = vignetteImage.color;
        color.a = currentIntensity;
        vignetteImage.color = color;
    }

    public void SetIntensity(float intensity)
    {
        targetIntensity = Mathf.Clamp01(intensity);
    }

    public void TriggerHorrorEffect()
    {
        targetIntensity = maxIntensity;
        Invoke("ResetIntensity", 3.0f);
    }

    void ResetIntensity()
    {
        targetIntensity = defaultIntensity;
    }
}
