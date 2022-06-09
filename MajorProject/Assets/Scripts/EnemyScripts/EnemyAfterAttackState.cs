using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAfterAttackState : EnemyState
{
    protected NavMeshAgent agent;
    protected float curTime;


    public EnemyAfterAttackState(IStateMachineController _controller, NavMeshAgent _agent) : base(_controller)
    {
        
    }

    public override void EnterState()
    {
        myEnemy.CurrentState = EnemyController.EEnemyStates.ES_AfterAttacking;
        curTime = myEnemy.GetAfterAttackIdleTime();
    }

    public override void ExitState()
    {
        myEnemy.SetAttackDoneFlag();
    }

    public override void UpdateState()
    {
        
    }
}
