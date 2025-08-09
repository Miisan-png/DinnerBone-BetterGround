using UnityEngine;

public class SpiderLegImpact : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform legImpact;       // The moving leg object
    [SerializeField] private GameObject impactVFX;      // Particle prefab

    [Header("Positions")]
    [SerializeField] private float startY = 4.93f;
    [SerializeField] private float impactY = 0.04f;

    [Header("Timing")]
    [SerializeField] private float waitBeforeImpact = 60f;
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 startPos;
    private Vector3 impactPos;

    void Start()
    {
        if (!legImpact) legImpact = transform; // If nothing set, use self
        startPos = new Vector3(legImpact.position.x, startY, legImpact.position.z);
        impactPos = new Vector3(legImpact.position.x, impactY, legImpact.position.z);

        legImpact.position = startPos;
        StartCoroutine(ImpactRoutine());
    }

    System.Collections.IEnumerator ImpactRoutine()
    {
        // Wait 60 seconds before attack
        yield return new WaitForSeconds(waitBeforeImpact);

        // Move leg down to impact
        while (Vector3.Distance(legImpact.position, impactPos) > 0.01f)
        {
            legImpact.position = Vector3.MoveTowards(legImpact.position, impactPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Spawn impact particle
        if (impactVFX)
            Instantiate(impactVFX, legImpact.position, Quaternion.identity);

        // Move leg back up
        while (Vector3.Distance(legImpact.position, startPos) > 0.01f)
        {
            legImpact.position = Vector3.MoveTowards(legImpact.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
