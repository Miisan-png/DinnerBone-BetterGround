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
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            
            if (!isAttacking)
            {
                StartCoroutine(ExecuteAttackPattern());
            }
        }
    }

    private IEnumerator ExecuteAttackPattern()
    {
        if (spiderLegs.Length == 0) yield break;
        
        isAttacking = true;
        
        int numberOfLegsToAttack = Random.Range(minLegsPerAttack, maxLegsPerAttack + 1);
        numberOfLegsToAttack = Mathf.Min(numberOfLegsToAttack, spiderLegs.Length);
        
        List<int> selectedLegs = SelectLegsForAttack(numberOfLegsToAttack);
        
        if (attackSimultaneously)
        {
            foreach (int legIndex in selectedLegs)
            {
                TriggerLegAttack(legIndex);
            }
        }
        else if (attackInSequence)
        {
            foreach (int legIndex in selectedLegs)
            {
                TriggerLegAttack(legIndex);
                float delay = Random.Range(minDelayBetweenLegs, maxDelayBetweenLegs);
                yield return new WaitForSeconds(delay);
            }
        }
        else if (randomAttackOrder)
        {
            foreach (int legIndex in selectedLegs)
            {
                TriggerLegAttack(legIndex);
                float delay = Random.Range(minDelayBetweenLegs, maxDelayBetweenLegs);
                yield return new WaitForSeconds(delay);
            }
        }
        
        yield return new WaitForSeconds(2f);
        
        foreach (int legIndex in selectedLegs)
        {
            if (legIndex < spiderLegs.Length && spiderLegs[legIndex] != null)
            {
                spiderLegs[legIndex].EndAttack();
            }
        }
        
        isAttacking = false;
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
            return;
            
        SpiderLegEvents leg = spiderLegs[legIndex];
        
        leg.StartAttack();
        
        Animator legAnimator = leg.GetComponent<Animator>();
        if (legAnimator != null)
        {
            legAnimator.SetTrigger("Attack");
        }
    }

    public void ManualTriggerAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(ExecuteAttackPattern());
        }
    }

    public void ManualTriggerLeg(int legIndex)
    {
        if (legIndex >= 0 && legIndex < spiderLegs.Length)
        {
            TriggerLegAttack(legIndex);
        }
    }

    public void StopAllAttacks()
    {
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