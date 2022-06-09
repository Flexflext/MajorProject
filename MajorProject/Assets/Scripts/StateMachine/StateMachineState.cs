using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachineState
{
    protected IStateMachineController stateMachineController;

    public StateMachineState(IStateMachineController _controller)
    {
        stateMachineController = _controller;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();
}
