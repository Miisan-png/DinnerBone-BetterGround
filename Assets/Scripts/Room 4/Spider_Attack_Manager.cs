using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderAttackManager : MonoBehaviour
{
    [Header("Spider Legs")]
    [SerializeField] private SpiderLegEvents[] spiderLegs;
    
    [Header("Players")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private float minDelayBetweenLegs = 0.2f;
    [SerializeField] private float maxDelayBetweenLegs = 1f;
    [SerializeField] private int minLegsPerAttack = 1;
    [SerializeField] private int maxLegsPerAttack = 3;
    
    [Header("Attack Pattern")]
    [SerializeField] private bool randomAttackOrder = true;
    [SerializeField] private bool attackInSequence = false;
    [SerializeField] private bool attackSimultaneously = false;
    
    private bool isAttacking = false;
    private List<int> availableLegIndices = new List<int>();

    void Start()
    {
        if (player1 == null) player1 = FindPlayerByType(Player_Type.Luthe);
        if (player2 == null) player2 = FindPlayerByType(Player_Type.Cherie);
        
        Debug.Log($"[SpiderManager] Found Player1: {(player1 != null ? player1.name : "NULL")}, Player2: {(player2 != null ? player2.name : "NULL")}");
        Debug.Log($"[SpiderManager] Managing {spiderLegs.Length} spider legs");
        
        InitializeAvailableLegs();
        StartCoroutine(AttackLoop());
    }

    private Transform FindPlayerByType(Player_Type playerType)
    {
        Player_Controller[] players = FindObjectsByType<Player_Controller>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.Get_Player_Type() == playerType)
                return player.transform;
        }
        return null;
    }

    private void InitializeAvailableLegs()
    {
        availableLegIndices.Clear();
        for (int i = 0; i < spiderLegs.Length; i++)
        {
            availableLegIndices.Add(i);
        }
    }

    private IEnumerator AttackLoop()
    {
        Debug.Log("[SpiderManager] Starting attack loop");
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            
            if (!isAttacking)
            {
                Debug.Log("[SpiderManager] Initiating new attack sequence");
                StartCoroutine(ExecuteAttackPattern());
            }
            else
            {
                Debug.Log("[SpiderManager] Skipping attack - already attacking");
            }
        }
    }

    private IEnumerator ExecuteAttackPattern()
    {
        if (spiderLegs.Length == 0) 
        {
            Debug.LogWarning("[SpiderManager] No spider legs assigned!");
            yield break;
        }
        
        isAttacking = true;
        
        int numberOfLegsToAttack = Random.Range(minLegsPerAttack, maxLegsPerAttack + 1);
        numberOfLegsToAttack = Mathf.Min(numberOfLegsToAttack, spiderLegs.Length);
        
        Debug.Log($"[SpiderManager] Attacking with {numberOfLegsToAttack} legs");
        
        List<int> selectedLegs = SelectLegsForAttack(numberOfLegsToAttack);
        
        if (attackSimultaneously)
        {
            Debug.Log("[SpiderManager] Executing simultaneous attack");
            foreach (int legIndex in selectedLegs)
            {
                TriggerLegAttack(legIndex);
            }
        }
        else if (attackInSequence)
        {
            Debug.Log("[SpiderManager] Executing sequential attack");
            foreach (int legIndex in selectedLegs)
            {
                TriggerLegAttack(legIndex);
                float delay = Random.Range(minDelayBetweenLegs, maxDelayBetweenLegs);
                Debug.Log($"[SpiderManager] Waiting {delay}s before next leg");
                yield return new WaitForSeconds(delay);
            }
        }
        else if (randomAttackOrder)
        {
            Debug.Log("[SpiderManager] Executing random order attack");
            foreach (int legIndex in selectedLegs)
            {
                TriggerLegAttack(legIndex);
                float delay = Random.Range(minDelayBetweenLegs, maxDelayBetweenLegs);
                yield return new WaitForSeconds(delay);
            }
        }
        
        Debug.Log("[SpiderManager] Waiting for attacks to complete");
        yield return new WaitForSeconds(2f);
        
        Debug.Log("[SpiderManager] Ending all attacks");
        foreach (int legIndex in selectedLegs)
        {
            if (legIndex < spiderLegs.Length && spiderLegs[legIndex] != null)
            {
                spiderLegs[legIndex].EndAttack();
            }
        }
        
        isAttacking = false;
        Debug.Log("[SpiderManager] Attack sequence complete");
    }

    private List<int> SelectLegsForAttack(int numberOfLegs)
    {
        List<int> selectedLegs = new List<int>();
        List<int> tempAvailable = new List<int>(availableLegIndices);
        
        for (int i = 0; i < numberOfLegs && tempAvailable.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempAvailable.Count);
            selectedLegs.Add(tempAvailable[randomIndex]);
            tempAvailable.RemoveAt(randomIndex);
        }
        
        return selectedLegs;
    }

    private void TriggerLegAttack(int legIndex)
    {
        if (legIndex < 0 || legIndex >= spiderLegs.Length || spiderLegs[legIndex] == null)
        {
            Debug.LogError($"[SpiderManager] Invalid leg index: {legIndex}");
            return;
        }
            
        SpiderLegEvents leg = spiderLegs[legIndex];
        
        Debug.Log($"[SpiderManager] Triggering attack on leg {legIndex}: {leg.gameObject.name}");
        
        leg.StartAttack();
        
        Animator legAnimator = leg.GetComponent<Animator>();
        if (legAnimator != null)
        {
            Debug.Log($"[SpiderManager] Playing attack animation on {leg.gameObject.name}");
            legAnimator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogWarning($"[SpiderManager] No animator found on {leg.gameObject.name}");
        }
    }

    public void ManualTriggerAttack()
    {
        Debug.Log("[SpiderManager] Manual attack triggered");
        if (!isAttacking)
        {
            StartCoroutine(ExecuteAttackPattern());
        }
        else
        {
            Debug.Log("[SpiderManager] Cannot trigger manual attack - already attacking");
        }
    }

    public void ManualTriggerLeg(int legIndex)
    {
        Debug.Log($"[SpiderManager] Manual trigger leg {legIndex}");
        if (legIndex >= 0 && legIndex < spiderLegs.Length)
        {
            TriggerLegAttack(legIndex);
        }
        else
        {
            Debug.LogError($"[SpiderManager] Invalid manual trigger leg index: {legIndex}");
        }
    }

    public void StopAllAttacks()
    {
        Debug.Log("[SpiderManager] Stopping all attacks");
        StopAllCoroutines();
        isAttacking = false;
        
        foreach (var leg in spiderLegs)
        {
            if (leg != null)
            {
                leg.EndAttack();
            }
        }
        
        StartCoroutine(AttackLoop());
    }

    void OnDrawGizmosSelected()
    {
        if (spiderLegs != null)
        {
            for (int i = 0; i < spiderLegs.Length; i++)
            {
                if (spiderLegs[i] != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(spiderLegs[i].transform.position, 0.5f);
                    
                    Vector3 labelPos = spiderLegs[i].transform.position + Vector3.up * 1f;
                }
            }
        }
    }
}