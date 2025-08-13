using UnityEngine;
using System.Collections.Generic;

public class CobwebSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.1f; // How far they move
    [SerializeField] private float swaySpeed = 1f; // How fast they sway
    [SerializeField] private float randomDelay = 2f; // Stagger start times

    private List<Transform> webs = new List<Transform>();
    private Vector3[] basePositions;
    private float[] randomOffsets;

    void Start()
    {
        // Get all child web meshes
        foreach (Transform child in transform)
        {
            if (child.GetComponent<MeshFilter>() != null)
            {
                webs.Add(child);
            }
        }

        // Store original positions and set random timings
        basePositions = new Vector3[webs.Count];
        randomOffsets = new float[webs.Count];
        
        for (int i = 0; i < webs.Count; i++)
        {
            basePositions[i] = webs[i].localPosition;
            randomOffsets[i] = Random.Range(0f, 100f); // Different start phase
        }
    }

    void Update()
    {
        for (int i = 0; i < webs.Count; i++)
        {
            if (webs[i] == null) continue;
            
            // Gentle sine wave movement with per-web variation
            float sway = Mathf.Sin((Time.time * swaySpeed) + randomOffsets[i]) * swayAmount;
            
            // Apply movement (mostly horizontal with slight vertical)
            webs[i].localPosition = basePositions[i] + 
                new Vector3(sway * 0.7f, Mathf.Abs(sway * 0.3f), 0);
        }
    }
}