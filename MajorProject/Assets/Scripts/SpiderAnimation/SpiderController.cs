using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    public void SetControll()
    {
        isGettingControlled = true;
        spiderAgent.isStopped = true;
        spiderAgent.enabled = false;

        movement.enabled = true;
        gameLogic.enabled = true;
    }

    public void GiveUpControll()
    {
        controller.SetPlayerMovementInput(Vector2.zero);

        isGettingControlled = false;
        spiderAgent.enabled = true;
        spiderAgent.isStopped = false;

        movement.enabled = false;
        gameLogic.enabled = false;
    }

    private void OnDeath()
    {
        HUD.Instance.SetSpiderLegStatus(-1, ELegStates.LS_Broken);
    }

    private void OnDeathReset()
    {
        HUD.Instance.SetSpiderLegStatus(-1, ELegStates.LS_Normal);
    }
}
