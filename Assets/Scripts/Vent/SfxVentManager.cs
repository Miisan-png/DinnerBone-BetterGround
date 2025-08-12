using System;
using UnityEngine;

public class SfxVentManager : MonoBehaviour
{
    [SerializeField] private GameObject parentSfx;
    [SerializeField] private Vector3 detectPlayerBox;
    [SerializeField] private LayerMask playerLayer;

    private void OnEnable()
    {
        Player_State_Manager.OnPlayerRespawn += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        Player_State_Manager.OnPlayerRespawn -= HandlePlayerDeath;
    }

    public void DisableVentSfx()
    {
        parentSfx.SetActive(false);
    }

    public void EnableVentSfx()
    {
        parentSfx.SetActive(true);
    }

    public void CheckPlayerInVent()
    {
        bool playerInvent = Physics.CheckBox(transform.position, detectPlayerBox, Quaternion.identity, playerLayer);
        Debug.Log(playerInvent);


        if (playerInvent)
        {
            EnableVentSfx();
        }
        else
        {
            DisableVentSfx();
        }
    }

    private void HandlePlayerDeath(Player_Controller controller)
    {
        Util.WaitNextFrame(this, () =>
        {
            if (controller.Get_Player_Type() == Player_Type.Cherie)
            {
                CheckPlayerInVent();
            }
        });

        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position, detectPlayerBox);
    }
}
