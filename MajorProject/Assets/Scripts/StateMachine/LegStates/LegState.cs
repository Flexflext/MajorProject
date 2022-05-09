using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LegState
{
    protected ProzeduralAnimationLogic legController;

    public LegState(ProzeduralAnimationLogic _controller)
    {
        legController = _controller;
    }

    public abstract IEnumerator C_MoveLegCoroutine(int _leg);

    public abstract void EnterLegState(int _leg);

    public abstract void ExitLegState(int _leg);
}
