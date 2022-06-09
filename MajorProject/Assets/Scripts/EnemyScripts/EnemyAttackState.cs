using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttackState : EnemyState
{
    protected NavMeshAgent agent;
    protected Vector3 hidingPosition;
    bool wasAttacking = false;

    public EnemyAttackState(IStateMachineController _controller, NavMeshAgent _agent) : base(_controller)
    {
        agent = _agent;
    }

    public override void EnterState()
    {
        wasAttacking = false;
        myEnemy.CurrentState = EnemyController.EEnemyStates.ES_Attacking;
        EnemyManager.Instance.SubscribeToAttacking(myEnemy);
        hidingPosition = myEnemy.FindNewHidingPosition();
    }

    public override void ExitState()
    {
        myEnemy.ResetHidingPosition();
        EnemyManager.Instance.UnSubscribeToAttacking(myEnemy);
    }

    public override void UpdateState()
    {
        if (myEnemy.CanAttack)
        {
            if (!wasAttacking)
	        {
                myEnemy.ResetHidingPosition();
                wasAttacking = true;
            }   

            //Attack
        }
        else
        {
            if (EnemyManager.Instance.CheckIfPositionCanSeePlayer(hidingPosition))
            {
                hidingPosition = myEnemy.FindNewHidingPosition();
            }

            //Find Cover
            agent.SetDestination(hidingPosition);
        }
    }
}
