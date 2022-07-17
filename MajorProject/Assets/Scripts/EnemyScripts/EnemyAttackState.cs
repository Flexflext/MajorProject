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

    private float timeTillNextAttack = 1f;
    private float curTimeTillNextAttack;

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
        myEnemy.SetAttackAnimations(1);
    }

    public override void ExitState()
    {
        myEnemy.ResetHidingPosition();
        EnemyManager.Instance.UnSubscribeToAttacking(myEnemy);
        myEnemy.SetAttackAnimations(0);
    }

    public override void UpdateState()
    {
        if (myEnemy.CanAttack)
        {
            //Attack
            if (!wasAttacking)
            {
                myEnemy.ResetHidingPosition();
                myEnemy.SetAttackAnimations(1);
                wasAttacking = true;
            }

            if (curTimeTillNextAttack <= 0)
            {
                myEnemy.Shoot();
                curTimeTillNextAttack = timeTillNextAttack;
            }
            else
            {
                curTimeTillNextAttack -= Time.deltaTime;
            }

            
            attackPosition = myEnemy.FindAttackPosition(attackingIdx);
            agent.SetDestination(attackPosition);
            

            myEnemy.StayAwayFromPlayer(1);
        }
        else
        {
            if (EnemyManager.Instance.CheckIfPositionCanSeePlayer(hidingPosition))
            {
                hidingPosition = myEnemy.FindNewHidingPosition(); //* Time.deltaTime;
                myEnemy.ResetHidingPosition();
            }

            //Find Cover
            agent.SetDestination(hidingPosition);
            myEnemy.StayAwayFromPlayer(0.5f);
        }

        myEnemy.transform.rotation = myEnemy.GetLookToPlayerRotation();


    }
}
