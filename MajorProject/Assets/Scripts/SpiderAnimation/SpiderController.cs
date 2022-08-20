using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Spider Controller Class for Playground
/// </summary>
public class SpiderController : MonoBehaviour
{
    private bool isGettingControlled = false;
    private NavMeshAgent spiderAgent;
    private SpiderGameLogic gameLogic;
    private ProzeduralAnimationLogic animLogic;
    private ThirdPersonSpiderMovement movement;
    private SpiderBodyRotationController controller;

    private void Awake()
    {      
        spiderAgent = GetComponent<NavMeshAgent>();
        gameLogic = GetComponent<SpiderGameLogic>();
        movement = GetComponent<ThirdPersonSpiderMovement>();
        controller = GetComponent<SpiderBodyRotationController>();
        animLogic = GetComponentInChildren<ProzeduralAnimationLogic>();
    }

    private void Start()
    {
        LevelManager.Instance.SpiderSubscribe(this);
        animLogic.AddDeathEventListener(OnDeath);
        animLogic.AddDeathResetEventListener(OnDeathReset);
        GiveUpControll();
    }

    private void Update()
    {
        if (!isGettingControlled)
        {
            if (spiderAgent.isOnNavMesh)
            {
                 spiderAgent.SetDestination(EnemyManager.Instance.PlayerPosition);
	        }
        }
    }

    private void OnDestroy()
    {
        animLogic.RemoveDeathEventListener(OnDeath);
        animLogic.RemoveDeathResetEventListener(OnDeathReset);
    }

    /// <summary>
    /// Give Controll to the Spider
    /// </summary>
    public void SetControll()
    {
        isGettingControlled = true;
        if (spiderAgent.enabled)
        {
            spiderAgent.isStopped = true;
            spiderAgent.enabled = false;
        }
        

        movement.SetPlayerStartControll();
        gameLogic.enabled = true;
    }

    /// <summary>
    /// Give Up Controll
    /// </summary>
    public void GiveUpControll()
    {
        controller.SetPlayerMovementInput(Vector2.zero);
        movement.SetPlayerStopControll();
        //movement.enabled = false;

        if (!spiderAgent.enabled)
        {
            spiderAgent.enabled = true;
            spiderAgent.isStopped = false;
        }

        
        isGettingControlled = false;

        gameLogic.enabled = false;
    }

    /// <summary>
    /// On death Event
    /// </summary>
    private void OnDeath()
    {
        HUD.Instance.SetSpiderLegStatus(-1, ELegStates.LS_Broken);
    }

    /// <summary>
    /// On Death Reset Event
    /// </summary>
    private void OnDeathReset()
    {
        HUD.Instance.SetSpiderLegStatus(-1, ELegStates.LS_Normal);
    }
}
