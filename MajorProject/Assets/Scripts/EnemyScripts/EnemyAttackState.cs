using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttackState : EnemyState
{
    protected NavMeshAgent agent;
    protected Vector3 hidingPosition;

    protected Vector3 attackPosition;
    bool wasAttacking = false;

    private int attackingIdx = 0;

    public EnemyAttackState(IStateMachineController _controller, NavMeshAgent _agent) : base(_controller)
    {
        agent = _agent;
    }

    public override void EnterState()
    {
        wasAttacking = false;
        myEnemy.CurrentState = EnemyController.EEnemyStates.ES_Attacking;
        attackingIdx = EnemyManager.Instance.SubscribeToAttacking(myEnemy);
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
            //Attack
            if (!wasAttacking)
            {
                myEnemy.ResetHidingPosition();
                wasAttacking = true;
            }

            myEnemy.Shoot();
            attackPosition = myEnemy.FindAttackPosition(attackingIdx);
            agent.SetDestination(attackPosition);

            if (Vector3.Dot(myEnemy.transform.forward, EnemyManager.Instance.PlayerPosition - myEnemy.transform.position) < 0)
            {
                myEnemy.transform.rotation = myEnemy.GetLookToPlayerRotation();
            }


        }
        else
        {
            if (EnemyManager.Instance.CheckIfPositionCanSeePlayer(hidingPosition))
            {
                hidingPosition = myEnemy.FindNewHidingPosition() * Time.deltaTime;
            }

            //Find Cover
            agent.SetDestination(hidingPosition);
        }

        myEnemy.StayAwayFromPlayer();
    }
}
