using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyState : StateMachineState
{
    protected EnemyController myEnemy;

    public EnemyState(IStateMachineController _controller) : base(_controller)
    {
        myEnemy = (EnemyController)_controller;
    }
}
