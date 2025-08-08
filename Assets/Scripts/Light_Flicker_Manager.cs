using UnityEngine;
using System.Collections.Generic;

public class LightFlickerManager : MonoBehaviour
{
    public List<Light> lightsToFlicker = new List<Light>();
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 0.1f;
    public bool isFlickering = true;

    private float flickerTimer;

    void Update()
    {
        if (isFlickering)
        {
            flickerTimer -= Time.deltaTime;
            if (flickerTimer <= 0)
            {
                foreach (Light light in lightsToFlicker)
                {
                    light.intensity = Random.Range(minIntensity, maxIntensity);
                }
                flickerTimer = flickerSpeed;
            }
        }
    }

    public void ToggleFullBrightness(bool turnOn)
    {
        isFlickering = !turnOn;

        if (turnOn)
        {
            foreach (Light light in lightsToFlicker)
            {
                light.intensity = maxIntensity;
            }
        }
    }
}
