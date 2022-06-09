using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWalkingState : EnemyState
{
    private Vector3 newWalkPosition = Vector3.zero;
    private float closeRange = 0.3f;
    private NavMeshAgent agent;

    float time = 5;

    public EnemyWalkingState(IStateMachineController _controller, NavMeshAgent _agent) : base(_controller)
    {
        agent = _agent;
    }

    public override void EnterState()
    {
        myEnemy.CurrentState = EnemyController.EEnemyStates.ES_Walking;
        newWalkPosition = myEnemy.FindNewWalkPosition();
        agent.SetDestination(newWalkPosition);
    }

    public override void ExitState()
    {
        agent.SetDestination(myEnemy.transform.position);
    }

    public override void UpdateState()
    {
        if (agent.remainingDistance <= closeRange)
        {
            myEnemy.ChangeIdleState();
        }
    }
}
