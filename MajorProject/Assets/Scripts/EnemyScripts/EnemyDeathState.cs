using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathState : EnemyState
{
    private float timeTillDespawn = 5f;

    public EnemyDeathState(IStateMachineController _controller) : base(_controller)
    {
    }

    public override void EnterState()
    {

    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if (timeTillDespawn <= 0)
        {
            myEnemy.Die();
        }
        else
        {
            timeTillDespawn -= Time.deltaTime;
        }
    }
}
