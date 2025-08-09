using UnityEngine;

public class Spider_Leg_Trigger : MonoBehaviour
{
    private SpiderLegEvents parentLeg;
    
    public void SetParentLeg(SpiderLegEvents leg)
    {
        parentLeg = leg;
    }
    
    void OnTriggerEnter(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player != null && parentLeg != null)
        {
            parentLeg.OnPlayerHitByTrigger(player);
        }
    }
}