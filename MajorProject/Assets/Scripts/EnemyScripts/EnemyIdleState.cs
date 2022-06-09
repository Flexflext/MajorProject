using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float timeTillChange;

    public EnemyIdleState(IStateMachineController _controller) : base(_controller)
    {
    }

    public override void EnterState()
    {
        myEnemy.CurrentState = EnemyController.EEnemyStates.ES_Idle;
        timeTillChange = myEnemy.GetIdleTimer();
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if (timeTillChange >= 0)
        {
            timeTillChange -= Time.deltaTime;
        }
        else
        {
            myEnemy.ChangeIdleState();
        }
    }
}
