using UnityEngine;
using System.Collections.Generic;

public class LightningFlicker : MonoBehaviour
{
    [Header("Main Flash Settings")]
    [Tooltip("The main bright flash light")]
    public Light mainFlash;
    [Tooltip("Normal intensity of main flash (should be lower than others)")]
    public float mainFlashBaseIntensity = 0.5f;
    [Tooltip("How much brighter main flash gets during strikes")]
    public float mainFlashMultiplier = 3f;

    [Header("Other Light Settings")]
    public float baseIntensity = 1f;
    public float flashIntensity = 3f;
    public float minFlashInterval = 3f;
    public float maxFlashInterval = 8f;
    public float flashDuration = 0.1f;
    [Range(0, 1)] public float doubleFlashChance = 0.3f;

    private List<Light> childLights = new List<Light>();
    private float nextFlashTime;
    private float[] originalIntensities;

    void Start()
    {
        // Get all child lights except the main flash
        foreach (Light light in GetComponentsInChildren<Light>(true))
        {
            if (light != mainFlash)
            {
                childLights.Add(light);
            }
        }

        // Store original intensities
        originalIntensities = new float[childLights.Count];
        for (int i = 0; i < childLights.Count; i++)
        {
            originalIntensities[i] = childLights[i].intensity;
        }

        // Set initial state (main flash dimmer than others)
        if (mainFlash != null)
        {
            mainFlash.intensity = mainFlashBaseIntensity;
        }
        
        nextFlashTime = Time.time + Random.Range(minFlashInterval, maxFlashInterval);
    }

    void Update()
    {
        if (Time.time >= nextFlashTime)
        {
            StartCoroutine(DoLightningFlash());
            nextFlashTime = Time.time + Random.Range(minFlashInterval, maxFlashInterval);
        }
    }

    System.Collections.IEnumerator DoLightningFlash()
    {
        // Initial flash
        SetLightIntensities(baseIntensity + flashIntensity);
        if (mainFlash != null)
        {
            mainFlash.intensity = mainFlashBaseIntensity + (flashIntensity * mainFlashMultiplier);
        }
        yield return new WaitForSeconds(flashDuration);

        // Return to normal
        SetLightIntensities(baseIntensity);
        if (mainFlash != null)
        {
            mainFlash.intensity = mainFlashBaseIntensity;
        }

        // Possible double flash
        if (Random.value < doubleFlashChance)
        {
            yield return new WaitForSeconds(flashDuration * 0.5f);
            
            // Secondary flash (smaller)
            SetLightIntensities(baseIntensity + flashIntensity * 0.7f);
            if (mainFlash != null)
            {
                mainFlash.intensity = mainFlashBaseIntensity + (flashIntensity * mainFlashMultiplier * 0.7f);
            }
            
            yield return new WaitForSeconds(flashDuration * 0.3f);
            
            // Return to normal again
            SetLightIntensities(baseIntensity);
            if (mainFlash != null)
            {
                mainFlash.intensity = mainFlashBaseIntensity;
            }
        }
    }

    void SetLightIntensities(float intensity)
    {
        foreach (Light light in childLights)
        {
            if (light != null)
            {
                light.intensity = intensity;
            }
        }
    }

    void OnDisable()
    {
        // Restore original intensities
        for (int i = 0; i < childLights.Count; i++)
        {
            if (childLights[i] != null)
            {
                childLights[i].intensity = originalIntensities[i];
            }
        }

        // Don't restore main flash - keep its special base intensity
        if (mainFlash != null)
        {
            mainFlash.intensity = mainFlashBaseIntensity;
        }
    }
}