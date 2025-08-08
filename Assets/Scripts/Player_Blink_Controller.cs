using UnityEngine;

public class Player_Blink_Controller : MonoBehaviour
{
    public SkinnedMeshRenderer targetMesh;
    public Material openEyesMaterial;
    public Material closedEyesMaterial;
    public float blinkInterval = 3f;
    public float blinkDuration = 0.1f;

    private float blinkTimer;
    private bool isBlinking;
    private float blinkEndTimer;

    void Start()
    {
        if (targetMesh == null)
            targetMesh = GetComponent<SkinnedMeshRenderer>();
            
        blinkTimer = blinkInterval;
    }

    void Update()
    {
        if (isBlinking)
        {
            blinkEndTimer -= Time.deltaTime;
            if (blinkEndTimer <= 0)
            {
                targetMesh.material = openEyesMaterial;
                isBlinking = false;
                blinkTimer = blinkInterval;
            }
        }
        else
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0)
            {
                targetMesh.material = closedEyesMaterial;
                isBlinking = true;
                blinkEndTimer = blinkDuration;
            }
        }
    }
}